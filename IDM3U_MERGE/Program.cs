using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace IDM3U_MERGE
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new Program();
            List<string> projectFiles = new List<string>();


            if (args.Length != 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (app.ProjectExist(args[i]))
                    {
                        projectFiles.Add(args[i]);
                    }
                }
                List<string> additionalProjectFiles = new List<string>();
                additionalProjectFiles = app.SelectProjectFile();
                foreach (string item in additionalProjectFiles)
                {
                    projectFiles.Add(item);
                }
            }
            else
            {
                List<string> additionalProjectFiles = new List<string>();
                additionalProjectFiles = app.SelectProjectFile();
                foreach (string item in additionalProjectFiles)
                {
                    projectFiles.Add(item);
                }
            }
            if (projectFiles == null || projectFiles.Count == 0)
            {
                return 0;
            }
            else
            {
                app.CreateDefaultDirectories();
                int projectNumber = 0; //Set to zero so we know it is the first project in the list
                int numberOfProjects = projectFiles.Count;//
                List<string> multiProjectImagesDirectories = new List<string>();
                List<string> multiProjectDirectories = new List<string>();
                List<string> projectNames = new List<string>();
                foreach (string item in projectFiles)
                {
                    string project = item;
                    projectNames.Add(project);
                    string projectImagesDirectory = "";
                    string projectDirectory = "";

                    Console.WriteLine("Merging: {0}", project);

                    projectDirectory = app.GetProjectDirectory(project);//Get the current project working directory in loop
                    multiProjectDirectories.Add(projectDirectory);//Add project Directory to access later
                    projectImagesDirectory = app.GetProjectImagesDirectory(project);//Get the current project working directory in loop
                    multiProjectImagesDirectories.Add(projectImagesDirectory);//Add project Directory to access later
                    
                    app.CreateProjectDirectories(project);

                    app.CopyProject(project);
                    string renamedProject = app.ProjectRename(project);
                    app.MoveFilesOutOfZip(renamedProject, projectDirectory, projectImagesDirectory, projectNumber);
                    app.ProjectDbConnectionImages(projectDirectory, projectNumber);
                    //app.MoveFilesToProject(renamedProject, projectDirectory, projectImagesDirectory);
                    //app.ProjectRename(renamedProject);
                    //app.DeleteDirectory(projectImagesDirectory);
                    //app.DeleteDirectory(projectDirectory);
                    app.ClearLists();
                    //app.ConvertIdimageFileToJpg();                    

                    projectNumber++;
                }

                //app.CreateConfig();
                //app.ReadConfig();
                //app.CreateDirectory();
                //app.CreateFile();
                //Console.Read();
                //app.ArchiveConfig();
                //app.SaveImage();
                //app.CopySaveData();
                //app.DeleteTmp();
            }
            Console.WriteLine("ID Maker 3 Utility - Trim tasked finished");
            Console.Read();
            return 1;
        }



        private FileSystem fileSystem;
        List<string> projectDbFileNames = new List<string>();
        List<string> projectDbRenamedFileNames = new List<string>();
        List<string> imageColumnNames = new List<string>();
        List<string> columnNames = new List<string>();
        const string tableName = "CardDB";
        const string photoOneColumn = "CustomDataField_Photo1";
        const string pictureOneColumn = "DefaultDataField_Picture1";
        const string pictureTwoColumn = "DefaultDataField_Picture2";
        const string pictureThreeColumn = "DefaultDataField_Picture3";
        const string pictureFourColumn = "DefaultDataField_Picture4";
        const string fingerprintColumn = "DefaultDataField_Fingerprint";
        const string signatureColumn = "DefaultDataField_Signature";

        //public List<string> ProjectFiles
        //{
        //    get { return projectFiles; }
        //    set { projectFiles = value; }
        //}


        public Program()
        {
            fileSystem = new FileSystem();
        }

        public bool ProjectExist(string project)
        {
            if (fileSystem.FileExists(project) == true)
            {
                return true;
            }
            else
                return false;
        }

        public List<string> SelectProjectFile()
        {
            List<string> projectFiles = new List<string>();
            bool done = false;
            while (done == false)
            {
                Console.WriteLine("Please enter path to project file or Skip to continue.");
                string projectFile = Console.ReadLine();


                if (projectFile.ToLower() == "skip")
                {
                    done = true;
                }
                else if (fileSystem.FileExists(projectFile) == true && fileSystem.GetExtension(projectFile) == ".idproject")
                {
                    projectFiles.Add(projectFile);
                }
            }
            return projectFiles;
        }

        private void CopyProject(string project)
        {
            string fileName = fileSystem.GetFileName(project);
            string destination = fileSystem.CurrentDirectory() + @"Backup\" + fileName;
            fileSystem.CopyFile(project, destination);
        }

        public string ProjectRename(string srcFilename)
        {
            if (srcFilename != null)
            {
                if (fileSystem.FileExists(srcFilename) == true)
                {
                    srcFilename = fileSystem.RenameFileExtIdproject(srcFilename);
                }
            }
            else
            {
                Console.WriteLine("Project does not exist!");
            }
            return srcFilename;
        }

        private bool CheckIfColumnExists(string tableName, string columnName, string projectDirectory)
        {

            using (var conn = new SQLiteConnection("data source=" + projectDirectory + "Database.sqlite;"))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("PRAGMA table_info({0})", tableName);
                var reader = cmd.ExecuteReader();
                int nameIndex = reader.GetOrdinal("Name");

                while (reader.Read())
                {
                    if (reader.GetString(nameIndex).Equals(columnName))
                    {
                        conn.Close();
                        return true;
                    }
                }
                conn.Close();
            }
            return false;
        }

        public void ProjectDbConnection(string projectDirectory)
        {
            string databaseFile = "Database.sqlite;";
            string databaseSource = "data source=" + projectDirectory + databaseFile;

            //if (!fileSystem.FileExists(databaseFile))
            //{
            //    SQLiteConnection.CreateFile(databaseFile);
            //}

            //string sqlSelectStatement = "SELECT * FROM " + tableName;

            using (var connection = new SQLiteConnection(databaseSource))
            {
                // Create a database command
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    //command.CommandText = sqlSelectStatement;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        //int i = 0;
                        while (reader.Read())
                        {
                            columnNames.Add(reader.GetString(0));
                            //i += 1;
                        }
                    }
                    connection.Close(); // Close the connection to the database
                }
            }

        }

        public void ProjectDbConnectionImages(string projectDirectory, int projectNumber)
        {
            string databaseFile = "Database.sqlite;";
            string databaseSource = "data source=" + projectDirectory + databaseFile;

            //if (!fileSystem.FileExists(databaseFile))
            //{
            //    SQLiteConnection.CreateFile(databaseFile);
            //}
            bool photoOneColumnExists = CheckIfColumnExists(tableName, photoOneColumn, projectDirectory);
            bool pictureOneColumnExists = CheckIfColumnExists(tableName, pictureOneColumn, projectDirectory);
            bool pictureTwoColumnExists = CheckIfColumnExists(tableName, pictureTwoColumn, projectDirectory);
            bool pictureThreeColumnExists = CheckIfColumnExists(tableName, pictureThreeColumn, projectDirectory);
            bool pictureFourColumnExists = CheckIfColumnExists(tableName, pictureFourColumn, projectDirectory);
            bool fingerprintColumnExists = CheckIfColumnExists(tableName, fingerprintColumn, projectDirectory);
            bool signatureColumnExists = CheckIfColumnExists(tableName, signatureColumn, projectDirectory);
            string sqlSelectStatement = "SELECT ";
            if (photoOneColumnExists == true)
            {
                imageColumnNames.Add(photoOneColumn);
                sqlSelectStatement += photoOneColumn + ", ";
            }
            if (pictureOneColumnExists == true)
            {
                imageColumnNames.Add(pictureOneColumn);
                sqlSelectStatement += pictureOneColumn + ", ";
            }
            if (pictureTwoColumnExists == true)
            {
                imageColumnNames.Add(pictureTwoColumn);
                sqlSelectStatement += pictureTwoColumn + ", ";
            }
            if (pictureThreeColumnExists == true)
            {
                imageColumnNames.Add(pictureThreeColumn);
                sqlSelectStatement += pictureThreeColumn + ", ";
            }
            if (pictureFourColumnExists == true)
            {
                imageColumnNames.Add(pictureFourColumn);
                sqlSelectStatement += pictureFourColumn + ", ";
            }
            if (fingerprintColumnExists == true)
            {
                imageColumnNames.Add(fingerprintColumn);
                sqlSelectStatement += fingerprintColumn + ", ";
            }
            if (signatureColumnExists == true)
            {
                imageColumnNames.Add(signatureColumn);
                sqlSelectStatement += signatureColumn + ", ";
            }

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
                        int i = 0;
                        string columnFileName = "";
                        while (reader.Read())
                        {
                            if (projectNumber > 0)
                                columnFileName += projectNumber;
                            if (photoOneColumnExists == true)
                                columnFileName += String.Format("{0}", reader[photoOneColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (pictureOneColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureOneColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (pictureTwoColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureTwoColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (pictureThreeColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureThreeColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (pictureFourColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureFourColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (fingerprintColumnExists == true)
                                columnFileName += String.Format("{0}", reader[fingerprintColumn]);
                                projectDbFileNames.Add(columnFileName);
                            if (signatureColumnExists == true)
                                columnFileName += String.Format("{0}", reader[signatureColumn]);
                                projectDbFileNames.Add(columnFileName);
                            i += 1;
                        }
                    }
                    connection.Close(); // Close the connection to the database
                }
            }

            using (var connection = new SQLiteConnection(databaseSource))
            {
                // Create a database command
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    //command.CommandText = sqlSelectStatement;
                    sqlSelectStatement = "";
                    foreach (var item in columnNames)
                    {

                    }

                    command.CommandText = "update CardDB set Info";
                    using (var reader = command.ExecuteReader())
                    {
                        int i = 0;
                        string columnFileName = "";



                        while (reader.Read())
                        {
                            if (projectNumber > 0)
                                columnFileName += projectNumber;
                            if (photoOneColumnExists == true)
                                columnFileName += String.Format("{0}", reader[photoOneColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (pictureOneColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureOneColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (pictureTwoColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureTwoColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (pictureThreeColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureThreeColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (pictureFourColumnExists == true)
                                columnFileName += String.Format("{0}", reader[pictureFourColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (fingerprintColumnExists == true)
                                columnFileName += String.Format("{0}", reader[fingerprintColumn]);
                            projectDbFileNames.Add(columnFileName);
                            if (signatureColumnExists == true)
                                columnFileName += String.Format("{0}", reader[signatureColumn]);
                            projectDbFileNames.Add(columnFileName);
                            i += 1;
                        }
                    }
                    connection.Close(); // Close the connection to the database
                }
            }

        }

        public List<string> FilesInZip(string zipFile)
        {
            string zipPath = fileSystem.GetFullPath(zipFile);
            List<string> zipEntries = new List<string>(); //Store File names extracted out of zip
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {

                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        zipEntries.Add(entry.FullName);
                    }
                }
            }
            return zipEntries;
        }

        public List<string> FilterFilesInZip(List<string> filesInZip)
        {
            foreach (var filtered in filesInZip.ToList())
            {
                if (filtered.Contains("Photo") || filtered.Contains("Picture") || filtered.Contains("Database.sqlite") || filtered.Contains("Signature") || filtered.Contains("Fingerprint"))
                    continue;
                else
                    filesInZip.Remove(filtered);
            }
            return filesInZip;
        }

        public void MoveFilesOutOfZip(string renamedProject, string projectDirectory, string projectImagesDirectory, int projectNumber)
        {
            string zipPath = renamedProject;
            List<string> zipEntries = FilesInZip(zipPath); //Get File names in zip
            List<string> filteredZipEntries = FilterFilesInZip(zipEntries);

            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {

                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {

                        if (filteredZipEntries.Contains(entry.FullName))
                        {
                            string fileName = entry.FullName;
                            if (projectNumber > 0)
                            {
                                fileName = projectNumber + fileName;
                            }
                            if (fileSystem.FileExists(projectDirectory + fileName) == false && entry.FullName.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase))//.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(projectDirectory, fileName));
                            }
                            else
                            {
                                if (fileSystem.FileExists(Path.Combine(projectImagesDirectory, fileName)) == false)
                                    entry.ExtractToFile(Path.Combine(projectImagesDirectory, fileName));
                            }
                        }
                        else
                            continue;
                    }
                    foreach (var item in filteredZipEntries)
                    {

                        GetNewEntry(archive, item);
                    }
                    archive.Dispose();
                }

                void GetNewEntry(ZipArchive archive, string item)
                {
                    var archiveFile = archive.GetEntry(item);
                    archiveFile?.Delete();
                }
            }
        }

        public void RebuildProjectFile()
        {

        }


        public void MoveFilesToProject(string renamedProjectFile, string projectDirectory, string projectImagesDirectory)//string zipPath, string sourceFile, string newEntry)
        {
            string sourceDatabaseFile = projectDirectory + "Database.sqlite";

            foreach (var item in projectDbFileNames)
            {
                string sourceImageFile = projectImagesDirectory + item;

                string newEntry = item;

                if (newEntry == "")
                    continue;
                else
                    fileSystem.MoveFilesIntoZip(renamedProjectFile, sourceImageFile, newEntry);
            }
            fileSystem.MoveFilesIntoZip(renamedProjectFile, sourceDatabaseFile, "Database.sqlite");
        }

        //public string ConfigFile
        //{
        //    get { return GetUserDataFolder() + args; }
        //}

        public void CreateProjectDirectories(string projectName)
        {
            projectName = fileSystem.GetFileNameWithoutExtension(projectName);
            fileSystem.CreateDirectory(projectName + @"\Images");

        }

        public string GetProjectDirectory(string projectName)
        {
            projectName = fileSystem.GetFileNameWithoutExtension(projectName);
            string projectDirectory = projectName + @"\";
            return projectDirectory;
        }

        public string GetProjectImagesDirectory(string projectName)
        {
            projectName = fileSystem.GetFileNameWithoutExtension(projectName);
            string projectImagesDirectory = projectName + @"\Images\";
            return projectImagesDirectory;
        }

        public void CreateDefaultDirectories()
        {
            fileSystem.CreateDirectory("Backup");
        }

        public void DeleteDirectory(string dirName)
        {
            if (fileSystem.DirectoryExists(dirName))
            {
                fileSystem.DeleteDirectory(dirName);
            }
        }

        public void ClearLists()
        {
            projectDbFileNames.Clear();
        }

        static bool HasJpegHeader(string filename)
        {
            using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                UInt16 soi = br.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
                UInt16 marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FF01)

                return soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
            }
        }

        private void ConvertIdimageFileToJpg()
        {
            StripExcessDataFromJpeg(@"C:\Users\Eric\Desktop\images\Picture3_1.idimage");
        }

        //Remove the excess data in given file and returns the new location of the modified file and
        //returns an empty string if no file was created.
        public static void StripExcessDataFromJpeg(string fileLocation)
        {
            //if no image is found in the file, it will return this error message
            string newFileName = "";
            BinaryReader br = new BinaryReader(File.Open(fileLocation, FileMode.Open));
            try
            {
                bool done = false;
                long count = 0;
                //The file must be read until the end of the file (in which case there was no image)
                //or until the jpeg file is found.
                while ((count < br.BaseStream.Length) && !done)
                {
                    //has to be read one at a time so not to consume more than needed
                    count++;
                    if (br.ReadByte() == 0xFF)
                    {
                        count++;
                        if (br.ReadByte() == 0xD8)
                        {
                            done = true;
                            newFileName = fileLocation + ".jpg";
                            WriteJpegBinaryToFile(newFileName, br);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("File format not found.");
                //File name set back to empty to indicate the file wasn't created.
                newFileName = "";
            }
            br.Close();
            //return newFileName;
        }
        private static void WriteJpegBinaryToFile(string fileName, BinaryReader br)
        {
            FileStream fs = File.Create(fileName);
            //write back the jpeg header already consumed in the stream
            byte[] jpegHeader = { 0xFF, 0xD8 };
            fs.Write(jpegHeader, 0, jpegHeader.Length);
            long count = 0;
            while (count < br.BaseStream.Length)
            {
                long bytesToRead = 1024; //Read 1kb at a time, increase if dealing with larger files.
                if (bytesToRead + count > br.BaseStream.Length)
                {
                    bytesToRead = br.BaseStream.Length - count;
                }
                byte[] bytes = new byte[bytesToRead];
                br.Read(bytes, 0, bytes.Length);
                count += bytesToRead;
                fs.Write(bytes, 0, bytes.Length);
            }
            fs.Close();
        }

        //public string GetFolderByName(FolderNames name)
        //{
        //    return folders[(int)name];
        //    //return fileSystem.GetUserDataFolder() + folders[(int)name];//Creates folders in user appdata folder\IDM3U
        //}
    }
}
