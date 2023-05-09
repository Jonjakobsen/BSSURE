using Bssure.Pages;
using Bssure.ViewModels;
using System.ComponentModel;
using Microsoft.Maui.Controls.Hosting;
using CommunityToolkit.Maui; // Nuget needs to be version 1.2.0 to work, not 5.0.0. Jonsnyegren bruger 2.0.0, og det virker også.
using Bssure;
using Bssure.Services;

namespace Bssure;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

        // Initialise the toolkit
        //builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();
        builder.UseMauiCommunityToolkit();
        builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
#if ANDROID
        builder.Services.AddSingleton<BLEservice>();
#endif 
        builder.Services.AddSingleton<IMQTTService, MqttService>();


        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();
        builder.Services.AddSingleton<MeasurementPage>();
        builder.Services.AddSingleton<MeasurementPageViewModel>();
        //builder.Services.AddSingleton<PopUpBLE>();
        //builder.Services.AddSingleton<PopUpBLEViewModel>();


        //builder.Services.AddSingleton registers a singleton service for the specified type, meaning that only one instance of the service will be created and shared across the entire application.
        //So, builder.Services.AddSingleton<Class>() will register Class as a singleton service in the dependency injection container.
        //Whenever a component requests an instance of AviewmodelClass, the same instance will be returned.

        

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
        builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
        builder.Services.AddSingleton<IMap>(Map.Default);
        return builder.Build();
	}

}
