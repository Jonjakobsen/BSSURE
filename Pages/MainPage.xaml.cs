using Bssure.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Security.Cryptography.X509Certificates;

namespace Bssure.Pages;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class MainPage : ContentPage
{ 
    public MainPage(MainPageViewModel vm)
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = vm;
        
    }

}
