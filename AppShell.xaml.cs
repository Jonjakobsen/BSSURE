using Bssure.Pages;

namespace Bssure;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(MeasurementPage), typeof(MeasurementPage));


    }
}
