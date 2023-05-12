using Bssure.Events;
using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bssure.Pages;

public partial class MeasurementPage : ContentPage
{
    public MeasurementPage(IMeasurement vm)
    {
        InitializeComponent();
        BindingContext = vm;              
        
    }


    

}


