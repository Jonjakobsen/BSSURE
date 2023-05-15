using Bssure.Pages;
using Bssure.ViewModels;
using System.ComponentModel;
using Microsoft.Maui.Controls.Hosting;
using CommunityToolkit.Maui; // Nuget needs to be version 1.2.0 to work, not 5.0.0. Jonsnyegren bruger 2.0.0, og det virker også.
using Bssure;
using Bssure.Services;
using Syncfusion.Maui.Core.Hosting;
using System.Diagnostics;

namespace Bssure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        Thread.Sleep(100);

        var builder = MauiApp.CreateBuilder();

        try
        {
            // Initialise the toolkit
            //builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();
            builder.UseMauiCommunityToolkit();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
#if ANDROID

            Debug.WriteLine("Adding BLE service");
            builder.Services.AddSingleton<BLEservice>();
#endif
            Debug.WriteLine("Adding MQTT Service");
            builder.Services.AddSingleton<IMQTTService, MqttService>();

            Debug.WriteLine("Adding Main Page");
            builder.Services.AddSingleton<MainPage>();
            Debug.WriteLine("Adding MainPageViewModel");
            builder.Services.AddSingleton<MainPageViewModel>();

            Debug.WriteLine("Adding MeasurementPage");
            builder.Services.AddSingleton<MeasurementPage>();
            Debug.WriteLine("Adding Measurement Page View Model");
            builder.Services.AddSingleton<IMeasurement, MeasurementPageViewModel>();

            Debug.WriteLine("Adding Decoding Byte Array");
            builder.Services.AddSingleton<IDecoder, DecodingByteArray>();


            //builder.Services.AddSingleton registers a singleton service for the specified type, meaning that only one instance of the service will be created and shared across the entire application.
            //So, builder.Services.AddSingleton<Class>() will register Class as a singleton service in the dependency injection container.
            //Whenever a component requests an instance of AviewmodelClass, the same instance will be returned.



            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<IMap>(Map.Default);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error during Create Application " + ex);
        }
        return builder.Build();
    }

}
