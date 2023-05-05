using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;

namespace Bssure.Pages;

public partial class MeasurementPage : ContentPage
{
    MeasurementPageViewModel viewModel;
    public MeasurementPage(MeasurementPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        viewModel = vm;
               
        
    }


    

}


