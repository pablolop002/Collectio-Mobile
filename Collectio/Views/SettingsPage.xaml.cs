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
            AppCenterStatus.IsChecked = Preferences.Get("AppCenter", true);
        }

        private void Backup_OnClicked(object sender, EventArgs e)
        {
            try
            {
                if (App.DataRepo.CreateBackup())
                {
                    Shell.Current.DisplayAlert("", "Backup created", Strings.Ok);
                }
                else
                {
                    Shell.Current.DisplayAlert(Strings.Error, "Error creating backup", Strings.Ok);
                }
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Backup");
                Shell.Current.DisplayAlert(Strings.Error, "Error creating backup", Strings.Ok);
            }
        }

        private void Comments_OnClicked(object sender, EventArgs e)
        {
        }

        private async void AppCenterStatus_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            await AppCenter.SetEnabledAsync(e.Value);
            Preferences.Set("AppCenter", e.Value);
        }
    }
}