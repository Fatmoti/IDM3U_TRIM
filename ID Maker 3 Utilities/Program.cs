using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID_Maker_3_Utilities
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new Program();
            var sqlLite = new SqLite();
            List<string> projectFiles = new List<string>();
            
            
            if (args.Length != 0)
            {
                bool quiet = false;
                for(int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower() == "quiet")
                        quiet = true;
                    if (app.ProjectExist(args[i]))
                    {
                        projectFiles.Add(args[i]);
                    }

                }
                if (quiet == false)
                {
                    List<string> additionalProjectFiles = new List<string>();
                    additionalProjectFiles = app.SelectProjectFile();
                    foreach (string item in additionalProjectFiles)
                    {
                        projectFiles.Add(item);
                    }
                }
            }
            else
            {
                List<string> additionalProjectFiles = new List<string>();
                additionalProjectFiles = app.SelectProjectFile();
                foreach(string item in additionalProjectFiles)
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
                Console.Clear();
                app.CreateDefaultDirectories();
                int projectNumber = 0; //Set to zero so we know it is the first project in the list
                foreach (string item in projectFiles)
                {
                    List<string> projectDbFileNames = new List<string>();
                    string project = item;
                    string projectImagesDirectory = "";
                    string projectDirectory = "";
                    string projectMoved = "";

                    if(projectNumber == 0)
                    Console.WriteLine("==============================================================================");
                    Console.WriteLine("Project Size            : {0}", app.ProjectSize(project));
                    Console.WriteLine("Trimming                : {0}", project);

                    projectDirectory = app.GetProjectDirectory(project);
                    projectImagesDirectory = app.GetProjectImagesDirectory(project);
                    projectMoved = projectDirectory + app.GetProjectName(project);
                    //int numberOfProjects = projectFiles.Count;

                    app.CreateProjectDirectories(project);
                    app.CopyProject(project, projectMoved);//Move project to Current Directory->project directory
                    app.RenameProjectBackup(project);//Add .backup to create a backup
                    string renamedProject = app.ProjectRename(projectMoved); //Rename .idproject file to .zip                                       
                    app.MoveFilesOutOfZip(renamedProject, projectDirectory, projectImagesDirectory, projectNumber); //Move .idimage files and .sqlite file out of zip
                    projectDbFileNames = app.ProjectDbConnection(projectDirectory); //Get image file names out of sqlite database
                    app.MoveFilesToProject(renamedProject, projectDirectory, projectImagesDirectory, projectDbFileNames); //Move only images from .sqlite file back into zip file.
                    app.ProjectRename(renamedProject); //Rename .zip file to .idproject
                    app.CopyProject(projectMoved, project); //Copy trimmed project to back to orignal location
                    app.DeleteDirectory(projectImagesDirectory); //Delete image files and directory
                    app.DeleteDirectory(projectDirectory);  //Delete project named directory
                    Console.WriteLine("Project Size            : {0}", app.ProjectSize(project));                   
                    Console.WriteLine("==============================================================================");
                    projectDbFileNames.Clear(); //Clear list for next project
                    projectNumber++; //Increment project number as we have finished with 1st project
                }
            }
            Console.WriteLine("ID Maker 3 Utility - Trim tasked finished");
            Console.Read();
            return 1;            
        }



        private FileSystem fileSystem;
        
        int projectImagesCountStart = 0;
        int projectImagesCountEnd = 0;
        private string newDatabaseName;


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

        private void CopyProject(string project, string destination)
        {
            //string fileName = fileSystem.GetFileName(project);
            if (!fileSystem.FileExists(destination))
            {
                fileSystem.CopyFile(project, destination);
                Console.WriteLine("Moving file locally     : Finished");
            }
        }

        string GetProjectName(string project)
        {
           return fileSystem.GetFileName(project);
        }

        private void RenameProjectBackup(string project)
        {
            string uniqueProjectName = string.Concat(
            Path.GetFileNameWithoutExtension(project),
            $"{DateTime.Now:_yyyy-MM-dd_hh-mm-ss}",
            Path.GetExtension(project)
            );
            if(fileSystem.GetDirectoryName(project) != "")
                uniqueProjectName = fileSystem.GetDirectoryName(project) + @"\" + uniqueProjectName;
            fileSystem.MoveFile(project, uniqueProjectName);
            fileSystem.ChangeExtension(uniqueProjectName, ".backup");
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
            projectImagesCountStart = filteredZipEntries.Count - 1;
            Console.WriteLine("Images in Project       : {0}", projectImagesCountStart);
            int projectImagesCountDown = projectImagesCountStart;
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Open))
            {
                
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    Console.Write("Extracting Images       : ");
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (projectImagesCountDown > 0)
                        {
                            projectImagesCountDown = projectImagesCountDown - 1;
                            CountDown(projectImagesCountDown);
                            Console.Write("\b\b\b\b\b\b\b\b");
                        }
                        else
                        {
                            Console.Write("Finished");
                            Console.Write("\b\b\b\b\b\b\b\b");
                        }
                        if (filteredZipEntries.Contains(entry.FullName))
                        {
                            if (fileSystem.FileExists(projectDirectory + entry.FullName) == false && entry.FullName.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase))//.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase))
                            {
                                entry.ExtractToFile(Path.Combine(projectDirectory, entry.FullName));
                                var databaseName = projectDirectory + entry.FullName;
                                newDatabaseName = projectDirectory + entry.FullName + ".backup";
                                fileSystem.CopyFile(databaseName, newDatabaseName);
                            }
                            else
                            {                                
                                if(fileSystem.FileExists(Path.Combine(projectImagesDirectory, entry.FullName)) == false)
                                    entry.ExtractToFile(Path.Combine(projectImagesDirectory, entry.FullName));
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


        public void MoveFilesToProject(string renamedProjectFile, string projectDirectory, string projectImagesDirectory, List<string> projectDbFileNames)//string zipPath, string sourceFile, string newEntry, list of images in sqlite DB)
        {
            string sourceDatabaseFile = projectDirectory + "Database.sqlite";
            //List<string> projectDbFileNames = new List<string>();
            projectImagesCountEnd = projectDbFileNames.Count;
            int orphandedImagesRemoved = projectImagesCountStart - projectImagesCountEnd;
            Console.Write("\nArchiving Images        : ");
            int images = 0;
            foreach (var item in projectDbFileNames)
            {            
                if (images < projectImagesCountEnd)
                {
                    images++;
                    CountDown(images);
                    Console.Write("\b\b\b\b\b\b\b\b");
                }
                if(images == projectImagesCountEnd)
                {
                    Console.Write("Finished");
                    Console.Write("\b\b\b\b\b\b\b\b");
                }
                string sourceImageFile = projectImagesDirectory + item;
                
                string newEntry = item;

                if (newEntry == "")
                    continue;
                else
                    fileSystem.MoveFilesIntoZip(renamedProjectFile, sourceImageFile, newEntry);
            }
            fileSystem.MoveFilesIntoZip(renamedProjectFile, sourceDatabaseFile, "Database.sqlite");
            Console.WriteLine("\nOrphaned Images Removed : {0}", orphandedImagesRemoved);
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

        public void CountDown(int startingValue)
        {
            Console.Write("{0:D8}", startingValue);
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

        public string ProjectSize(string projectName)
        {
            string projectSize = fileSystem.GetFileSize(projectName);
            return projectSize;
        }

        private void ConvertIdimageFileToJpg(string fileName)
        {
            if(HasJpegHeader(fileName))
                StripExcessDataFromJpeg(fileName);
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
            }
            return false;
        }

        public List<string> ProjectDbConnection(string projectDirectory)
        {
            List<string> projectDbFileNames = new List<string>();

            string databaseFile = "Database.sqlite.backup"; //Doesn't always close connection in time.
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
