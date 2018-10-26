using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;


namespace ID_Maker_3_Utilities
{
    public class FileSystem
    {
        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public string CurrentDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory() + @"\";
            return currentDirectory;
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path, bool recursive = true)
        {
            Directory.Delete(path, recursive);
        }

        public void MoveDirectory(string srcPath, string destPath)
        {
            Directory.Move(srcPath, destPath);
        }

        public string[] GetFilesInDirectory(string dirPath)
        {
            string[] filePaths = Directory.GetFiles(dirPath);
            return filePaths;
        }

        public string[] GetFilesInDirectory(string dirPath, string searchString)
        {
            string[] filePaths = Directory.GetFiles(dirPath);
            return filePaths;
        }

        public string[] GetFilesInDirectory(string dirPath, string searchString, bool searchOption)
        {
                string[] filePaths = Directory.GetFiles(dirPath, searchString, SearchOption.AllDirectories);
                return filePaths;
        }

        //public string[] GetFilesInDirectory(string dirPath, string searchString, bool searchOption.)
        //{
        //    string[] filePaths = Directory.GetFiles(dirPath, searchString);
        //    return filePaths;
        //}

        public string RenameFileExtIdproject(string srcFileName)
        {
            string destFileName;
            if (Path.GetExtension(srcFileName) == ".idproject")
                destFileName = Path.ChangeExtension(srcFileName, ".zip");
            else
                destFileName = Path.ChangeExtension(srcFileName, ".idproject");
            File.Move(srcFileName, destFileName);
            return destFileName;
        }

        public void ChangeExtension(string path, string extension)
        {
            //string destFileName = string.Concat(
            //Path.GetFileNameWithoutExtension(path),
            //DateTime.Now.ToString("yyyyMMddHHmmssfff"),
            //Path.GetExtension(path)
            //);
            string destFileName = Path.ChangeExtension(path, extension);
            File.Move(path, destFileName);
        }

        public string GetFullPath(string file)
        {
            return Path.GetFullPath(file);
        }

        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public char DirectorySeparatorChar
        {
            get { return Path.DirectorySeparatorChar; }
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }


        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public void WriteAllText(string path, string text)
        {
            File.WriteAllText(path, text);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void WriteAllLines(string path, string[] lines)
        {
            File.WriteAllLines(path, lines);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path);
        }

        public void CopyFile(string srcPath, string destPath)
        {
            File.Copy(srcPath, destPath, true);
        }

        public void MoveFile(string srcFile, string destFile)
        {
            File.Move(srcFile, destFile);
        }

        public void MoveFilesIntoZip(string zipPath, string sourceFile, string newEntry)
        {
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(sourceFile, newEntry);
            }
        }

        public string GetFileSize(string fileName)
        {
            long len = new FileInfo(fileName).Length;
            string fileSize = BytesToString(len);
            return fileSize;
        }

        public string GetUserDataFolder()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir += @"\IDM3U\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

    }
}
