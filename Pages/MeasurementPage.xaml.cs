using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bssure.Pages;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class MeasurementPage : ContentPage
{
    public MeasurementPage(IMeasurement vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }


    

}


