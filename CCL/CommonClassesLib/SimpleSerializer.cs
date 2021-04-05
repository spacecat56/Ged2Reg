using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace CommonClassesLib
{
    public class SimpleSerializer<TT>
    {
        public Type[] ExtraTypes { get; set; }

        public TT Load(string path)
        {
            if (path == null)
                path = GetDefaultPath();
            if (!File.Exists(path))
                return default(TT);
            DataContractSerializer dcs = new DataContractSerializer(typeof(TT), ExtraTypes);
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                {
                    TT rv = (TT)dcs.ReadObject(reader);
                    return rv;
                }
            }
        }

        public bool Persist(string path, TT victim)
        {
            if (path == null)
                path = GetDefaultPath();

            DataContractSerializer dcs = new DataContractSerializer(typeof(TT));
            using (BufferedStream bs = new BufferedStream(File.Create(path)))
            {
                XmlWriterSettings xset = new XmlWriterSettings()
                {
                    Indent = true,
                    NewLineHandling = NewLineHandling.Entitize
                };
                using (XmlWriter xw = XmlWriter.Create(bs, xset))
                {
                    dcs.WriteObject(xw, victim);
                }
            }
            return true;
        }

        /// <summary>
        /// By default the instance will be de/serialized from/to the appdata directory named for the 
        /// current assembly, in a file named for the class being serialized.
        /// </summary>
        /// <returns></returns>
        public string GetDefaultPath()
        {
            string path =
                $"{ApplicationInfo.ApplicationFolder}\\{ApplicationInfo.ApplicationName}\\{typeof(TT).Name}.xml";
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }
    }
}
