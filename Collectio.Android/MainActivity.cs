using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase;
using Xamarin.Forms.Platform.Android.AppLinks;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Collectio.Droid
{
    [Activity(Label = "Collectio", Icon = "@mipmap/icon", RoundIcon ="@mipmap/icon", Theme = "@style/MainTheme",
        MainLauncher = true, LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    [IntentFilter(new[] {Android.Content.Intent.ActionView},
        Categories = new[] {Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable},
        DataScheme = "collectio")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            FirebaseApp.InitializeApp(this);
            AndroidAppLinks.Init(this);

            LoadApplication(new App());
            Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>()
                .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }
    }
}