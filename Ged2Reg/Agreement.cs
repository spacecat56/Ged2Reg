using System;
using System.IO;
using System.Runtime.Serialization;
using CommonClassesLib;

namespace Ged2Reg
{
    public enum StateOfPlay
    {
        None,
        EUL,
        Authorship
    }

    [DataContract]
    public class Agreement
    {
        #region persistence and constructor
        public static Agreement Load(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            if (!File.Exists(path))
                throw new FileNotFoundException();
            SimpleSerializer<Agreement> ser = new SimpleSerializer<Agreement>();
            Agreement rv = ser.Load(path);
            return rv;
        }

        public Agreement Save(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }
            SimpleSerializer<Agreement> ser = new SimpleSerializer<Agreement>();
            ser.Persist(path, this);
            return this;
        }

        private static string GetPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"West Leitrim Software\Ged2Reg\AgreementRecord.xml");
        }

        public Agreement()
        {
            AgreedUser = Environment.UserName;
        }
        #endregion

        [DataMember] public DateTime? AgreedOn { get; set; }
        [DataMember] public string AgreedUser { get; set; }
        [DataMember] public string AgreedEul { get; set; }
        [DataMember] public string AgreedAuthorship { get; set; }
        [DataMember] public StateOfPlay Status { get; set; } = StateOfPlay.None;

    }
}
