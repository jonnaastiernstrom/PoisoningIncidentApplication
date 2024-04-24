using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.LifecycleEvents;


#if ANDROID
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace PoisoningIncidentApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Platform-specific configuration, including Android permissions
#if ANDROID
            builder.ConfigureLifecycleEvents(lifecycle =>
            {
                // Add Android Lifecycle events here
                lifecycle.AddAndroid(androidLifecycle =>
                {
                    androidLifecycle.OnCreate((activity, savedInstanceState) =>
                    {
                        // Check and request CALL_PHONE permission
                        if (ContextCompat.CheckSelfPermission(activity, Android.Manifest.Permission.CallPhone) != (int)Permission.Granted)
                        {
                            ActivityCompat.RequestPermissions(activity, new[] { Android.Manifest.Permission.CallPhone }, 0);
                        }
                    });
                });
            });
#endif

            return builder.Build();
        }
    }
}
