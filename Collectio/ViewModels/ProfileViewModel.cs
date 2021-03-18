using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private User _user;

        public ICommand AppleLogInCommand { get; }

        public ICommand GoogleLogInCommand { get; }

        public ICommand MicrosoftLogInCommand { get; }

        public ICommand SelectImageCommand { get; }

        public ICommand SettingsCommand { get; }

        public ICommand ApiKeysCommand { get; }

        public User User
        {
            get => _user;
            private set => SetProperty(ref _user, value);
        }

        public bool NotLoggedIn
        {
            get => !LoggedIn;
            set => LoggedIn = value;
        }

        public bool LoggedIn
        {
            get => Preferences.Get("LoggedIn", false);
            set
            {
                if (Preferences.Get("LoggedIn", false) == value) return;
                Title = LoggedIn ? Strings.Profile : Strings.Login;
                Preferences.Set("LoggedIn", value);
                OnPropertyChanged();
            }
        }

        public ProfileViewModel()
        {
            AppleLogInCommand = new AsyncCommand(AppleLogIn);
            GoogleLogInCommand = new AsyncCommand(GoogleLogIn);
            MicrosoftLogInCommand = new AsyncCommand(MicrosoftLogIn);
            SelectImageCommand = new AsyncCommand(SelectImage);
            SettingsCommand = new Command(SettingsPage);
            ApiKeysCommand = new AsyncCommand(ApiKeysPage);
            Title = LoggedIn ? Strings.Profile : Strings.Login;
            if (LoggedIn) Task.Run(async () => _user = await App.DataRepo.GetUser()).Wait();
        }

        private static void SettingsPage()
        {
            Xamarin.Forms.Shell.Current.GoToAsync("settings");
        }

        private static async Task ApiKeysPage()
        {
            App.Token = await SecureStorage.GetAsync("Token");
            await Xamarin.Forms.Shell.Current.GoToAsync("api-keys");
        }

        private static async Task SelectImage()
        {
            var selection = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.ProfileImage, Strings.Cancel,
                null, Strings.Camera, Strings.Gallery);

            if (selection == null || selection == Strings.Cancel) return;
            if (selection == Strings.Camera)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var photo = await MediaPicker.CapturePhotoAsync();
                        if (photo == null) return;

                        var imageName = photo.FileName;
                        var imageStream = new MemoryStream();

                        var stream = await photo.OpenReadAsync();
                        await stream.CopyToAsync(imageStream);
                        stream.Close();
                        stream.Dispose();

                        FileSystemUtils.SaveFileFromStream(imageStream, imageName);
                        // Save to server
                    }
                    catch (PermissionException ex)
                    {
                        await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
            else if (selection == Strings.Gallery)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var photo = await MediaPicker.PickPhotoAsync();
                        if (photo == null) return;

                        var imageName = photo.FileName;
                        var imageStream = new MemoryStream();

                        var stream = await photo.OpenReadAsync();
                        await stream.CopyToAsync(imageStream);
                        stream.Close();
                        stream.Dispose();

                        FileSystemUtils.SaveFileFromStream(imageStream, imageName);
                        // Save to server
                    }
                    catch (PermissionException ex)
                    {
                        await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }

        private async Task AppleLogIn()
        {
            try
            {
                WebAuthenticatorResult authResult;

                if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13)
                {
                    authResult = await AppleSignInAuthenticator.AuthenticateAsync();
                }
                else
                {
                    authResult = await WebAuthenticator.AuthenticateAsync(
                        new Uri(string.Format(RestServiceUtils.RestUrl, "/login/apple")),
                        new Uri("collectio://"));
                }

                var accessToken = authResult?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken)) return;
                await ProcessLogin(accessToken);
            }
            catch (TaskCanceledException ex)
            {
                AppCenterUtils.ReportException(ex, "loginAppleCancelled");
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "loginApple");
            }
            finally
            {
                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    {"Method", "Apple"},
                    {"Correct", LoggedIn.ToString()}
                });
            }
        }

        private async Task GoogleLogIn()
        {
            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(string.Format(RestServiceUtils.RestUrl, "/login/google")),
                    new Uri("collectio://"));

                var accessToken = authResult?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken)) return;
                await ProcessLogin(accessToken);
            }
            catch (TaskCanceledException ex)
            {
                AppCenterUtils.ReportException(ex, "loginGoogleCancelled");
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "loginGoogle");
            }
            finally
            {
                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    {"Method", "Google"},
                    {"Correct", LoggedIn.ToString()}
                });
            }
        }

        private async Task MicrosoftLogIn()
        {
            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(string.Format(RestServiceUtils.RestUrl, "/login/microsoft")),
                    new Uri("collectio://"));

                var accessToken = authResult?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken)) return;
                await ProcessLogin(accessToken);
            }
            catch (TaskCanceledException ex)
            {
                AppCenterUtils.ReportException(ex, "loginMicrosoftCancelled");
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "loginMicrosoft");
            }
            finally
            {
                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    {"Method", "Microsoft"},
                    {"Correct", LoggedIn.ToString()}
                });
            }
        }

        private async Task ProcessLogin(string accessToken)
        {
            await SecureStorage.SetAsync("Token", accessToken);
            await App.DataRepo.RestService.InsertToken();
            LoggedIn = true;
            //User = App.DataRepo.GetUser();
            //App.DataRepo.UpdateToken(accessToken, DeviceInfo.Model, DeviceInfo.Name);
        }
    }
}