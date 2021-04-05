using System;

namespace CommonClassesLib
{
    public static class ApplicationInfo
    {
        private static string _myApplicationName;
        public static string ApplicationName
        {
            get { return _myApplicationName ?? (_myApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name); }
            set { _myApplicationName = value; }
        }

        private static string _myApplicationFolder;
        public static string ApplicationFolder
        {
            get { return _myApplicationFolder ?? (_myApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)); }
            set { _myApplicationFolder = value; }
        }



    }
}
