using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Collectio.ViewModels.BaseViewModels;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private User _user;
        private bool _notLoggedIn = !Preferences.Get("LoggedIn", false);
        private bool _loggedIn = Preferences.Get("LoggedIn", false);

        public ICommand AppleLogInCommand { get; }

        public ICommand GoogleLogInCommand { get; }

        public ICommand MicrosoftLogInCommand { get; }

        public ICommand SelectImageCommand { get; }

        public ICommand SettingsCommand { get; }

        public ICommand ApiKeysCommand { get; }

        public ICommand SaveUserCommand { get; }

        public ICommand LogOutCommand { get; }

        public User User
        {
            get => _user;
            private set => SetProperty(ref _user, value);
        }

        public bool NotLoggedIn
        {
            get => _notLoggedIn;
            set
            {
                if (SetProperty(ref _notLoggedIn, value))
                {
                    LoggedIn = !value;
                }
            }
        }

        public bool LoggedIn
        {
            get => _loggedIn;
            set
            {
                if (_loggedIn == value) return;
                Preferences.Set("LoggedIn", value);
                Title = value ? Strings.Profile : Strings.Login;
                if (SetProperty(ref _loggedIn, value))
                {
                    NotLoggedIn = !value;
                }
            }
        }

        public ProfileViewModel()
        {
            AppleLogInCommand = new AsyncCommand(AppleLogIn);
#if DEBUG
            GoogleLogInCommand = new AsyncCommand(DebugLogin);
#else
            GoogleLogInCommand = new AsyncCommand(GoogleLogIn);
#endif
            MicrosoftLogInCommand = new AsyncCommand(MicrosoftLogIn);
            SelectImageCommand = new AsyncCommand(SelectImage);
            SettingsCommand = new Command(SettingsPage);
            ApiKeysCommand = new AsyncCommand(ApiKeysPage);
            SaveUserCommand = new AsyncCommand(SaveUser);
            LogOutCommand = new AsyncCommand(LogOut);
            Title = LoggedIn ? Strings.Profile : Strings.Login;
            if (LoggedIn) Task.Run(async () => _user = await App.DataRepo.GetUser()).Wait();
        }

        private async Task LogOut()
        {
            IsBusy = true;

            var apiKey = await SecureStorage.GetAsync("Token");

            if (SecureStorage.Remove("Token"))
            {
                App.DataRepo.DeleteAllData(true);
#if !DEBUG
                await App.DataRepo.RemoveApikey(apiKey);
#endif
                LoggedIn = false;
            }

            IsBusy = false;
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

        private async Task SaveUser()
        {
            await App.DataRepo.UpdateUser(User);
        }

        private async Task SelectImage()
        {
            var selection = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.ProfileImage, Strings.Cancel,
                string.IsNullOrWhiteSpace(_user.Image) ? null : Strings.Delete, Strings.Camera, Strings.Gallery,
                Strings.Picker);

            if (selection == null || selection == Strings.Cancel) return;

            if (selection == Strings.Delete)
            {
                var toDelete = _user.File;
                _user.Image = null;

                if (await App.DataRepo.RemoveUserImage(_user))
                {
                    FileSystemUtils.DeleteImage(toDelete);
                    await FileSystemUtils.SaveDefaultProfile();
                }

                return;
            }

            FileResult photo = null;

            if (selection == Strings.Camera)
            {
                try
                {
                    photo = await PhotoCapture();
                    if (photo == null) return;

                    var imageName = photo.FileName;
                    var imageStream = new MemoryStream();

                    var stream = await photo.OpenReadAsync();
                    await stream.CopyToAsync(imageStream);
                    stream.Close();
                    stream.Dispose();

                    FileSystemUtils.SaveFileFromStream(imageStream, imageName);
                }
                catch (PermissionException ex)
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                }
            }
            else if (selection == Strings.Gallery)
            {
                try
                {
                    photo = await GallerySelector();
                    if (photo == null) return;

                    var imageName = photo.FileName;
                    var imageStream = new MemoryStream();

                    var stream = await photo.OpenReadAsync();
                    await stream.CopyToAsync(imageStream);
                    stream.Close();
                    stream.Dispose();

                    FileSystemUtils.SaveFileFromStream(imageStream, imageName);
                }
                catch (PermissionException ex)
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                }
            }
            else
            {
                try
                {
                    photo = await ImagePicker();
                    if (photo == null) return;

                    var imageName = photo.FileName;
                    var imageStream = new MemoryStream();

                    var stream = await photo.OpenReadAsync();
                    await stream.CopyToAsync(imageStream);
                    stream.Close();
                    stream.Dispose();

                    FileSystemUtils.SaveFileFromStream(imageStream, imageName);
                }
                catch (Exception ex)
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                }
            }

            if (photo == null) return;

            var image = _user.File;
            _user.Image = photo.FileName;
            if (await App.DataRepo.UpdateUser(_user))
            {
                if (!string.IsNullOrWhiteSpace(image))
                {
                    FileSystemUtils.DeleteImage(image);
                }
            }
        }

        private async Task AppleLogIn()
        {
            try
            {
                IsBusy = true;

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
                IsBusy = false;

                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    { "Method", "Apple" },
                    { "Correct", LoggedIn.ToString() }
                });
            }
        }

        private async Task GoogleLogIn()
        {
            try
            {
                IsBusy = true;

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
                IsBusy = false;

                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    { "Method", "Google" },
                    { "Correct", LoggedIn.ToString() }
                });
            }
        }

        private async Task MicrosoftLogIn()
        {
            try
            {
                IsBusy = true;

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
                IsBusy = false;

                Analytics.TrackEvent("Login", new Dictionary<string, string>
                {
                    { "Method", "Microsoft" },
                    { "Correct", LoggedIn.ToString() }
                });
            }
        }

        private async Task DebugLogin()
        {
            await ProcessLogin("1234");
        }

        private async Task ProcessLogin(string accessToken)
        {
            await SecureStorage.SetAsync("Token", accessToken);
            await App.DataRepo.RestService.InsertToken();

            User = await App.DataRepo.GetUser();

            if (!string.IsNullOrWhiteSpace(User.Image))
            {
                await FileSystemUtils.SaveFileFromServer(User.Image, User.ServerId);
            }
            else
            {
                await FileSystemUtils.SaveDefaultProfile();
            }

            if (string.IsNullOrWhiteSpace(User.Image))
            {
                await FileSystemUtils.SaveFileFromServer(User.Image, User.ServerId);
            }

            LoggedIn = true;

            await App.DataRepo.UpdateApikey(new Apikey
            {
                Device = DeviceInfo.Model,
                Token = accessToken,
                UserDeviceName = DeviceInfo.Name
            });

            if (await App.DataRepo.UploadAllData())
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert("correct", "AllDataUploaded", Strings.Ok);
            }
            else
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, "ErrorUploadingData", Strings.Ok);
            }
        }
    }
}