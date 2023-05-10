using Bssure.Pages;

namespace Bssure;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(MeasurementPage), typeof(MeasurementPage));
        Routing.RegisterRoute(nameof(PopUpBLE), typeof(PopUpBLE));
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTg4ODcxN0AzMjMxMmUzMTJlMzMzNURLdUFIYWpNNFVxTjV4MFYrSTkxOFM2Z2V2aGdKYkJSOVZXYmhjY2h2ZkU9");
    }
}