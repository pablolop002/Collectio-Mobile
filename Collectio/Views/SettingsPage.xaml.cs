using System;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            //Shell.SetTabBarIsVisible(this, false);
            AppCenterStatus.IsChecked = Preferences.Get("AppCenter", true);
        }

        private void Backup_OnClicked(object sender, EventArgs e)
        {
            try
            {
                Shell.Current.DisplayAlert(Strings.Backup,
                    App.DataRepo.CreateBackup() ? Strings.BackupCorrect : Strings.BackupError, Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "CreateBackup");
                Shell.Current.DisplayAlert(Strings.Backup, Strings.BackupError, Strings.Ok);
            }
        }

        private void RestoreBackup_OnClicked(object sender, EventArgs e)
        {
            try
            {
                Shell.Current.DisplayAlert(Strings.RestoreBackup,
                    App.DataRepo.RestoreBackup() ? Strings.RestoreBackupCorrect : Strings.RestoreBackupError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "RestoreBackup");
                Shell.Current.DisplayAlert(Strings.RestoreBackup, Strings.RestoreBackupError, Strings.Ok);
            }
        }

        private async void DeleteData_OnClicked(object sender, EventArgs e)
        {
            if(!await Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteDataDescription,
                Strings.DeleteData, Strings.Cancel)) return;
            
            try
            {
                await Shell.Current.DisplayAlert(Strings.DeleteData,
                    App.DataRepo.DeleteAllData() ? Strings.DeleteDataCorrect : Strings.DeleteDataError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "DeleteData");
                await Shell.Current.DisplayAlert(Strings.DeleteData, Strings.DeleteDataError, Strings.Ok);
            }
        }

        private void DeleteCache_OnClicked(object sender, EventArgs e)
        {
            FileSystemUtils.ClearTempPath();
            
            if (!Preferences.Get("LoggedIn", false))
            {
                Shell.Current.DisplayAlert(Strings.Error, Strings.DisabledFunction, Strings.Ok);
                return;
            }
            
            try
            {
                Shell.Current.DisplayAlert(Strings.DeleteCache,
                    FileSystemUtils.DeleteCache() ? Strings.DeleteCacheCorrect : Strings.DeleteCacheError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "DeleteCache");
                Shell.Current.DisplayAlert(Strings.DeleteCache, Strings.DeleteCacheError, Strings.Ok);
            }
        }

        private void Comments_OnClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("comments");
        }

        private async void AppCenterStatus_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            await AppCenter.SetEnabledAsync(e.Value);
            Preferences.Set("AppCenter", e.Value);
        }
    }
}