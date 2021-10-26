using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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

            AppCenter.Start("android=APP_CENTER_DROID;ios=APP_CENTER_IOS",
                typeof(Analytics), typeof(Crashes));
            AppCenter.LogLevel = LogLevel.Verbose;
#if DEBUG
            AppCenter.SetEnabledAsync(false);
#else
            AppCenter.SetEnabledAsync(Preferences.Get("AppCenter", true));
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetLang();
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            base.OnAppLinkRequestReceived(uri);

            if (!Preferences.Get("LoggedIn", false)) return;

            var query = uri.Query.Split('&')
                .Select(item => item.Split('='))
                .ToDictionary(s => s[0], s => s[1]);

            if (!query.ContainsKey("userId")) return;
            if (!query.ContainsKey("collectionId")) return;

            Shell.Current.GoToAsync(query.ContainsKey("itemId")
                ? $"//collections/items?owner=false&collection={query["collectionId"]}"
                : $"//collections/items?owner=false&collection={query["collectionId"]}/item?item={query["itemId"]}");
        }

        private static void SetLang()
        {
            Thread.CurrentThread.CurrentUICulture = Collectio.Resources.Culture.Strings.Culture =
                Preferences.Get("lang", CultureInfo.InstalledUICulture.TwoLetterISOLanguageName) switch
                {
                    "en" => new CultureInfo("en"),
                    "ca" => new CultureInfo("ca"),
                    //"eu" => new CultureInfo("eu"),
                    _ => new CultureInfo("es")
                };
        }
    }
}