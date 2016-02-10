using System;
using System.IO;

namespace Engine
{
    class SqlConnector
    {
        public String Server { get; }
        public String Database { get; }
        public String Username { get; }
        public String Password { get; }

        public SqlConnector()
        {
            this.Server = GetServer();
            this.Database = GetDBName();
            this.Username = GetUsername();
            this.Password = GetPassword();
        }

        /// If configuration file already exists returns username, null otherwise
        private static string GetUsername()
        {
            if (SqlConnector.ConfigFileExists() == false)
                return null;

            string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\mssql.conf");
            return lines[0].Replace("USERNAME: ", "");
        }
        
        /// If configuration file already exists returns password, null otherwise
        private static string GetPassword()
        {
            if (SqlConnector.ConfigFileExists() == false)
                return null;

            string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\mssql.conf");
            return lines[1].Replace("PASSWORD: ", "");
        }

        /// If configuration file already exists returns db name, null otherwise
        private static string GetDBName()
        {
            if (SqlConnector.ConfigFileExists() == false)
                return null;

            string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\mssql.conf");
            return lines[2].Replace("DBNAME: ", "");
        }

        /// If configuration file already exists returns server, null otherwise.
        private static string GetServer()
        {
            if (SqlConnector.ConfigFileExists() == false)
                return null;

            string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + @"\mssql.conf");
            return lines[3].Replace("SERVER: ", "");
        }

        /// Returns true if configuration file already exists
        private static bool ConfigFileExists()
        {
            return File.Exists(Directory.GetCurrentDirectory() + @"\mssql.conf");
        }
    }
}
