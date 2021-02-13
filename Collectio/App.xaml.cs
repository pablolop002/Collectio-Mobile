using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Collectio.Repositories;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Collectio
{
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class App : Application
    {
        public static DataRepository DataRepo { get; private set; }
        
        public static string Token { get; set; }

        public App()
        {
            InitializeComponent();

            SetLang();
            DataRepo = new DataRepository();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (DeviceInfo.DeviceType != DeviceType.Physical) return;

            AppCenter.Start("android=APPCENTER_DROID;ios=APPCENTER_IOS",
                typeof(Analytics), typeof(Crashes));
            AppCenter.LogLevel = LogLevel.Verbose;
#if DEBUG
            AppCenter.SetEnabledAsync(false);
#else
            AppCenter.SetEnabledAsync(Preferences.Get("AppCenter", true));
            //AppCenter.SetUserId("256-characters");
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetLang();
        }

        private static void SetLang()
        {
            Thread.CurrentThread.CurrentUICulture = Collectio.Resources.Culture.Strings.Culture =
                Preferences.Get("lang", CultureInfo.InstalledUICulture.TwoLetterISOLanguageName) switch
                {
                    "en" => new CultureInfo("en"),
                    "ca" => new CultureInfo("ca"),
                    _ => new CultureInfo("es")
                };
        }
    }
}