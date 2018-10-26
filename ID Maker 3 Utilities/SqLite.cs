using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ID_Maker_3_Utilities
{
    class SqLite
    {

        const string tableName = "CardDB";
        const string photoOneColumn = "CustomDataField_Photo1";
        const string pictureOneColumn = "DefaultDataField_Picture1";
        const string pictureTwoColumn = "DefaultDataField_Picture2";
        const string pictureThreeColumn = "DefaultDataField_Picture3";
        const string pictureFourColumn = "DefaultDataField_Picture4";
        const string fingerprintColumn = "DefaultDataField_Fingerprint";
        const string signatureColumn = "DefaultDataField_Signature";

        private bool CheckIfColumnExists(string tableName, string columnName, string databaseSource)
        {

            using (var connection = new SQLiteConnection(databaseSource))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();
                    //var cmd = connection.CreateCommand();
                    command.CommandText = string.Format("PRAGMA table_info({0})", tableName);
                    var reader = command.ExecuteReader();
                    int nameIndex = reader.GetOrdinal("Name");

                    while (reader.Read())
                    {
                        if (reader.GetString(nameIndex).Equals(columnName))
                        {
                            connection.Close();
                            return true;
                        }
                    }
                    connection.Close();
                }
                connection.Close();
            }
            return false;
        }

        public List<string> ProjectDbConnection(string projectDirectory)
        {
            List<string> projectDbFileNames = new List<string>();

            string databaseFile = "Database.sqlite;";
            string databaseSource = "data source=" + projectDirectory + databaseFile;


            bool photoOneColumnExists = CheckIfColumnExists(tableName, photoOneColumn, databaseSource);
            bool pictureOneColumnExists = CheckIfColumnExists(tableName, pictureOneColumn, databaseSource);
            bool pictureTwoColumnExists = CheckIfColumnExists(tableName, pictureTwoColumn, databaseSource);
            bool pictureThreeColumnExists = CheckIfColumnExists(tableName, pictureThreeColumn, databaseSource);
            bool pictureFourColumnExists = CheckIfColumnExists(tableName, pictureFourColumn, databaseSource);
            bool fingerprintColumnExists = CheckIfColumnExists(tableName, fingerprintColumn, databaseSource);
            bool signatureColumnExists = CheckIfColumnExists(tableName, signatureColumn, databaseSource);
            string sqlSelectStatement = "SELECT ";
            if (photoOneColumnExists == true)
                sqlSelectStatement += photoOneColumn + ", ";
            if (pictureOneColumnExists == true)
                sqlSelectStatement += pictureOneColumn + ", ";
            if (pictureTwoColumnExists == true)
                sqlSelectStatement += pictureTwoColumn + ", ";
            if (pictureThreeColumnExists == true)
                sqlSelectStatement += pictureThreeColumn + ", ";
            if (pictureFourColumnExists == true)
                sqlSelectStatement += pictureFourColumn + ", ";
            if (fingerprintColumnExists == true)
                sqlSelectStatement += fingerprintColumn + ", ";
            if (signatureColumnExists == true)
                sqlSelectStatement += signatureColumn + ", ";

            sqlSelectStatement = sqlSelectStatement.TrimEnd(',', ' ');
            sqlSelectStatement += " FROM " + tableName + " WHERE ";
            if (photoOneColumnExists == true)
                sqlSelectStatement += photoOneColumn + " IS NOT NULL AND ";
            if (pictureOneColumnExists == true)
                sqlSelectStatement += pictureOneColumn + " IS NOT NULL AND ";
            if (pictureTwoColumnExists == true)
                sqlSelectStatement += pictureTwoColumn + " IS NOT NULL AND ";
            if (pictureThreeColumnExists == true)
                sqlSelectStatement += pictureThreeColumn + " IS NOT NULL AND ";
            if (pictureFourColumnExists == true)
                sqlSelectStatement += pictureFourColumn + " IS NOT NULL AND ";
            if (fingerprintColumnExists == true)
                sqlSelectStatement += fingerprintColumn + " IS NOT NULL AND ";
            if (signatureColumnExists == true)
                sqlSelectStatement += signatureColumn + " IS NOT NULL AND ";
            sqlSelectStatement = sqlSelectStatement.TrimEnd('A', 'N', 'D', ' ');

            using (var connection = new SQLiteConnection(databaseSource))
            {
                // Create a database command
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    command.CommandText = sqlSelectStatement;
                    using (var reader = command.ExecuteReader())
                    {
                        //int i = 0;
                        while (reader.Read())
                        {
                            if (photoOneColumnExists == true)
                                if (String.Format("{0}", reader[photoOneColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[photoOneColumn]));
                            if (pictureOneColumnExists == true)
                                if (String.Format("{0}", reader[pictureOneColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[pictureOneColumn]));
                            if (pictureTwoColumnExists == true)
                                if (String.Format("{0}", reader[pictureTwoColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[pictureTwoColumn]));
                            if (pictureThreeColumnExists == true)
                                if (String.Format("{0}", reader[pictureThreeColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[pictureThreeColumn]));
                            if (pictureFourColumnExists == true)
                                if (String.Format("{0}", reader[pictureFourColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[pictureFourColumn]));
                            if (fingerprintColumnExists == true)
                                if (String.Format("{0}", reader[fingerprintColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[fingerprintColumn]));
                            if (signatureColumnExists == true)
                                if (String.Format("{0}", reader[signatureColumn]) != "")
                                    projectDbFileNames.Add(String.Format("{0}", reader[signatureColumn]));
                            //i += 1;
                        }
                    }
                    connection.Close(); // Close the connection to the database
                }
            }
            return projectDbFileNames;
        }

    }
}
