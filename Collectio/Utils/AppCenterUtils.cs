using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

namespace Collectio.Utils
{
    public static class AppCenterUtils
    {
        public static void ReportException(Exception ex, string location)
        {
            Crashes.TrackError(ex, new Dictionary<string, string>()
            {
                {"Category", location},
                {"Message", ex.Message},
                {"Source", ex.Source},
            });
        }
    }
}