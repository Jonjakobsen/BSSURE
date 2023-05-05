using Bssure.Pages;
using Bssure.ViewModels;

namespace Bssure;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		
		MainPage = new AppShell();
	}
}
