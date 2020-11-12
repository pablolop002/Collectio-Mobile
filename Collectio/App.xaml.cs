using System.Collections.Generic;
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
    public partial class App : Application
    {
        public static DataRepository DataRepo { get; private set; }

        public App()
        {
            InitializeComponent();

            Xamarin.Forms.Device.SetFlags(new List<string>()
            {
                "SwipeView_Experimental",    // Opciones deslizantes
                "CarouselView_Experimental", // Carrusel
                "Brush_Experimental",        // Degradados
                "Shapes_Experimental"        // Formas geométricas
            });

            SetLang();

            DataRepo = new DataRepository();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();

#if !DEBUG
            if (DeviceInfo.DeviceType == DeviceType.Physical)
            {
                AppCenter.Start("android=;" +
                                "ios=",
                    typeof(Analytics), typeof(Crashes));
                AppCenter.LogLevel = LogLevel.Verbose;
                AppCenter.SetEnabledAsync(Preferences.Get("AppCenter", true));
                AppCenter.SetUserId("256 characters");
            }
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
                Preferences.Get("lang", null) switch
                {
                    "es" => new CultureInfo("es"),
                    "en" => new CultureInfo("en"),
                    "ca" => new CultureInfo("ca"),
                    _ => CultureInfo.InstalledUICulture
                };
        }
    }
}