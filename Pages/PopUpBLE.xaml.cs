using Bssure.ViewModels;

namespace Bssure.Pages;

public partial class PopUpBLE
{
    public PopUpBLE(PopUpBLEViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }




}