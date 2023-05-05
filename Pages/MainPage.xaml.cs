using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Security.Cryptography.X509Certificates;

namespace Bssure.Pages;

public partial class MainPage : ContentPage
{ 
    MainPageViewModel viewModel;
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = vm;
        viewModel= vm;
        
    }



    public async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (viewModel.UserIdEntry == "" || UserIdEntry == null)
        {
            await Shell.Current.DisplayAlert($"No User ID", $"Input User ID to continue", "OK");
        }
        else
        {
            try
            {
                await Shell.Current.Navigation.PushModalAsync(new MeasurementPage(new MeasurementPageViewModel()));
            }
            catch { await Shell.Current.DisplayAlert($"No User ID", $"Input User ID to continue", "OK"); }
        }
    }





}
