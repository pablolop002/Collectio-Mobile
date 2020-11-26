using System;
using System.IO;
using Xamarin.Essentials;

namespace Collectio.Utils
{
    public static class FileSystemUtils
    {
        #region Backup

        public static bool BackupDataAndDatabase(string databasePath)
        {
            try
            {
                var destDir = DeviceInfo.Platform == DevicePlatform.iOS
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : FileSystem.AppDataDirectory;

                destDir = Path.Combine(destDir, $"Backup_{DateTime.Now:yyyy-MM-dd_hh-mm-ss}");
                Directory.CreateDirectory(destDir);

                File.Copy(databasePath, Path.Combine(destDir, "collectio.db"), true);

                destDir = Path.Combine(destDir, "Images");
                var originDir = Path.Combine(FileSystem.AppDataDirectory, "Images");

                DirectoryCopy(originDir, destDir);

                return true;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Backup");
                return false;
            }
        }
        
        public static bool RestoreBackupDataAndDatabase(string databasePath)
        {
            try
            {
                /*var destDir = DeviceInfo.Platform == DevicePlatform.iOS
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : FileSystem.AppDataDirectory;

                destDir = Path.Combine(destDir, $"Backup_{DateTime.Now:yyyy-MM-dd_hh-mm-ss}");
                Directory.CreateDirectory(destDir);

                File.Copy(databasePath, Path.Combine(destDir, "collectio.db"), true);

                destDir = Path.Combine(destDir, "Images");
                var originDir = Path.Combine(FileSystem.AppDataDirectory, "Images");

                DirectoryCopy(originDir, destDir);*/

                return true;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "RestoreBackup");
                return false;
            }
        }

        #endregion

        #region Save

        public static string TempSave(MemoryStream stream, string fileName)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Temp");
            
            try
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, fileName);
                
                File.WriteAllBytes(path, stream.ToArray());

                return path;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "SaveFileTemp");
                return null;
            }
        }

        public static void ClearTempPath()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Temp");
            
            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "ClearTempPath");
            }
        }

        public static bool SaveFileFromPath(string originalPath, string fileName, int? collection = null, int? item = null)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            if (collection != null)
            {
                path = Path.Combine(path, $"Collection{collection.ToString()}");
                if (item != null) path = Path.Combine(path, $"Item{item.ToString()}");
            }

            try
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, fileName);
                
                File.Copy(originalPath, path, true);

                return true;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "FileCopy");
                return false;
            }
        }

        public static bool SaveFileFromStream(MemoryStream stream, string fileName, int? collection = null,
            int? item = null)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            if (collection != null)
            {
                path = Path.Combine(path, $"Collection{collection.ToString()}");
                if (item != null) path = Path.Combine(path, $"Item{item.ToString()}");
            }

            try
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, fileName);

                File.WriteAllBytes(path, stream.ToArray());

                stream.Close();
                stream.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "FileCreationFromStream");
                return false;
            }
        }

        #endregion

        #region Get

        public static string GetGroupImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";
            
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, "Groups");
            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                //SaveFileFromStream("", fileName);
            }

            return path;
        }
        
        public static string GetProfileImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";
            
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                //SaveFileFromStream("", fileName);
            }

            return path;
        }

        public static string GetCollectionImage(string fileName, int collection)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";
            
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, $"Collection{collection.ToString()}");
            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                //SaveFileFromStream("", fileName, collection);
            }

            return path;
        }

        public static string GetItemImage(string fileName, int itemId)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";
            
            var item = App.DataRepo.GetItem(itemId.ToString());

            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, $"Collection{item.CollectionId.ToString()}");
            path = Path.Combine(path, $"Item{itemId.ToString()}");
            path = Path.Combine(path, fileName);

            if (!File.Exists(path))
            {
                //SaveFileFromStream("", fileName, item.CollectionId, itemId);
            }

            return path;
        }

        #endregion

        #region Delete

        public static bool DeleteCache()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            Directory.Delete(path, true);
            return true;
        }
        
        public static bool DeleteAllData(string databasePath)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            Directory.Delete(path, true);
            File.Delete(databasePath);
            return true;
        }
        
        public static void DeleteImage(string imageDir)
        {
            File.Delete(imageDir);
        }

        public static void DeleteCollection(string collection)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, $"Collection{collection}");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static void DeleteItem(string collection, string item)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, $"Collection{collection}");
            path = Path.Combine(path, $"Item{item}");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        #endregion

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                return;
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // Copy subdirectories them and their contents to new location.
            foreach (var subDir in dirs)
            {
                var tempPath = Path.Combine(destDirName, subDir.Name);
                DirectoryCopy(subDir.FullName, tempPath);
            }
        }
    }
}