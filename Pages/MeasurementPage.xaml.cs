using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bssure.Pages;

public partial class MeasurementPage : ContentPage
{
    public MeasurementPage(MeasurementPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;              
        
    }


    

}


