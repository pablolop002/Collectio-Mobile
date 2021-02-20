using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Preferences.Get("LoggedIn", false))
            {
                Title = Strings.Profile;
                SigninInfo.IsVisible = SigninApple.IsVisible = SigninGoogle.IsVisible = false;
                ProfileImageBorder.IsVisible =
                    ProfileImage.IsVisible = ProfileData.IsVisible = LogoutButton.IsVisible = true;
            }
            else
            {
                Title = Strings.Login;
                ProfileImageBorder.IsVisible =
                    ProfileImage.IsVisible = ProfileData.IsVisible = LogoutButton.IsVisible = false;
                SigninInfo.IsVisible = SigninApple.IsVisible = SigninGoogle.IsVisible = true;
            }
        }

        /// <summary>
        /// Go to settings page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Settings_OnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("settings");
        }

        /// <summary>
        /// Apple Sign In
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Apple_OnClicked(object sender, EventArgs e)
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
                await SecureStorage.SetAsync("ApiKey", accessToken);
                Preferences.Set("LoggedIn", true);
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "loginApple");
            }
        }

        /// <summary>
        /// Google Sign In
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Google_OnClicked(object sender, EventArgs e)
        {
            try
            {
                var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri(string.Format(RestServiceUtils.RestUrl, "/login/google")),
                    new Uri("collectio://"));

                var accessToken = authResult?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken)) return;
                await SecureStorage.SetAsync("ApiKey", accessToken);
                Preferences.Set("LoggedIn", true);
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "loginGoogle");
            }
        }

        /// <summary>
        /// Select profile image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectImage_OnClicked(object sender, EventArgs e)
        {
            var selection = await Shell.Current.DisplayActionSheet("Strings.ProfileImage", Strings.Cancel, null,
                Strings.Camera, Strings.Gallery);

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
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
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
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }
    }
}