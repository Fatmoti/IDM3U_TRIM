using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;


namespace IDM3U_MERGE
{
    public class FileSystem
    {
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

        public void DeleteFile(string filePath)
        {
            //if(File.Exists(filePath) == true)
                File.Delete(filePath);
        }

        public void MoveFilesIntoZip(string zipPath, string sourceFile, string newEntry)
        {
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(sourceFile, newEntry);
            }
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
