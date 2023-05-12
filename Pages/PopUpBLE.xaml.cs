using Bssure.ViewModels;

namespace Bssure.Pages;

public partial class PopUpBLE 
{
    public PopUpBLE(IPopUpBLEViewModel vm) //PopUpBLEViewModel vm
    {
        InitializeComponent();
        BindingContext = vm;
    }




}