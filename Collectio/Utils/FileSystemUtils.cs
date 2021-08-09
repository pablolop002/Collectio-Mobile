using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                var destBaseDir = DeviceInfo.Platform == DevicePlatform.iOS
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : FileSystem.AppDataDirectory;

                var fileName = $"Backup_{DateTime.Now:yyyy-MM-dd_hh-mm-ss}";

                destBaseDir = Path.Combine(destBaseDir, fileName);
                Directory.CreateDirectory(destBaseDir);

                File.Copy(databasePath, Path.Combine(destBaseDir, "collectio.db"), true);

                var destDir = Path.Combine(destBaseDir, "Images");
                var originDir = Path.Combine(FileSystem.AppDataDirectory, "Images");

                DirectoryCopy(originDir, destDir);

                //ZipFile.CreateFromDirectory(destBaseDir, Path.Combine(destBaseDir, ".zip"));

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
                var originDir = DeviceInfo.Platform == DevicePlatform.iOS
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : FileSystem.AppDataDirectory;
                var dirs = new DirectoryInfo(originDir).GetDirectories("Backup_*-*-*_*-*-*");
                if (dirs.Length == 0)
                {
                    return false;
                }

                var directory = dirs.OrderByDescending(d => d.FullName).First();

                if (!directory.Exists || directory.GetFiles("collectio.db").Length == 0 ||
                    !directory.GetFiles("collectio.db").First().Exists) return false;

                // Delete old images and database
                File.Delete(databasePath);

                var baseDir = new DirectoryInfo(FileSystem.AppDataDirectory);
                if (baseDir.GetDirectories("Images").Length != 0 &&
                    baseDir.GetDirectories("Images").First().Exists)
                    baseDir.GetDirectories("Images").First().Delete();

                // Copy database
                directory.GetFiles("collectio.db").First().CopyTo(databasePath, true);

                // Check if backup has images directory and copy it
                if (directory.GetDirectories("Images").Length == 1 &&
                    directory.GetDirectories("Images").First().Exists)
                {
                    DirectoryCopy(directory.GetDirectories("Images").First().FullName,
                        Path.Combine(FileSystem.AppDataDirectory, "Images"));
                }

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

        public static string TempSave(MemoryStream stream, string fileName, bool withCompression = true)
        {
            /*if (withCompression)
            {
                if (fileName.EndsWith(".png"))
                {
                    stream = Xamarin.Forms.DependencyService.Get<INativeFunctions>().ConvertToJpeg(stream);
                    fileName = fileName.Replace(".png", ".jpg");
                }
                else
                {
                    stream = Xamarin.Forms.DependencyService.Get<INativeFunctions>().CompressJpeg(stream);
                }
            }*/

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

        public static bool SaveFileFromPath(string originalPath, string fileName, int? collection = null,
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

        public static async Task<bool> SaveCategoryImage(string fileName)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, "Categories");

            try
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, fileName);

                var response =
                    await App.DataRepo.RestService.GetImageRequest(string.Format(RestServiceUtils.RestUrl,
                        $"/categories/{fileName}"));

                if (response != null)
                {
                    File.WriteAllBytes(path, response.ToArray());

                    response.Close();
                    response.Dispose();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "CategoryImageCreationFromStream");
                return false;
            }
        }

        public static async Task<bool> SaveFileFromServer(string fileName, int? user, int? collection = null,
            int? item = null)
        {
            if (user == null)
            {
                return false;
            }

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

                var route = $"/images/user{user.ToString()}";

                if (collection != null)
                {
                    route += $"/collection{collection.ToString()}";

                    if (item != null)
                    {
                        route += $"/item{item.ToString()}";
                    }
                }

                route += $"/{fileName}";

                var response = await App.DataRepo.RestService.GetImageRequest(route);

                if (response != null)
                {
                    File.WriteAllBytes(path, response.ToArray());

                    response.Close();
                    response.Dispose();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "FileCreationFromServer");
                return false;
            }
        }

        public static async Task<bool> SaveDefaultProfile()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");

            try
            {
                Directory.CreateDirectory(path);
                path = Path.Combine(path, "default_image.png");

                var response = await App.DataRepo.RestService.GetImageRequest("/images/default_image.png");

                if (response != null)
                {
                    File.WriteAllBytes(path, response.ToArray());

                    response.Close();
                    response.Dispose();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "FileCreationFromServer");
                return false;
            }
        }

        #endregion

        #region Get

        public static string GetCategoryImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";

            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");
            path = Path.Combine(path, "Categories");
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

        public static bool DeleteAllData()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "Images");

            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "DeleteFolder");
            }

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