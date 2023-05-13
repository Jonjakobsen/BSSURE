using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bssure.Pages;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class MeasurementPage : ContentPage
{
    public MeasurementPage(MeasurementPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;              
        
    }


    

}


