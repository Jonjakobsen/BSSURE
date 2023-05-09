using Bssure.CortriumDevice;
using Bssure.Pages;
using Bssure.Services;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
//using Intents;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Bssure.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {

        public BLEservice ble { get; private set; } //This is the service that is injected into the viewmodel, that handles Plugin.Ble
        //public IMQTTService _mqttService { get; }
        public IRawDataService rawDataSender { get; }
        public ICommand BLEConnectCommand { get; }

        public ICommand SubmitUserIDCommand { get; }

        // Her starter viewproperties 

        private string _UserIdEntry;
        public string UserIdEntry
        {
            get => _UserIdEntry;
            set => SetProperty(ref _UserIdEntry, value);
        }






        public MainPageViewModel(BLEservice bluetoothLEService, IRawDataService rawDataService) //Dependency injection of the BLEservice is necessary in all viewmodel classes. Passed globally from singleton in mauiprogram.cs
        {

            ble = bluetoothLEService;
            //mqttService = _mqttService;
            rawDataSender = rawDataService;
            BLEConnectCommand = new RelayCommand(OnBLE_connectClicked);
            SubmitUserIDCommand = new RelayCommand(OnSubmitClicked);
            LoadUser();
        }


        private async void OnSubmitClicked()
        {
            if (UserIdEntry == "" || UserIdEntry == null)
            {
                await Application.Current.MainPage.DisplayAlert("No User ID", $"Input User ID to continue", "OK");

            }
            else
            {
                StoreUserId(UserIdEntry);
                //await Shell.Current.Navigation.PushModalAsync(new (new MeasurementPageViewModel()));
                await Shell.Current.GoToAsync(nameof(MeasurementPage), true);

            }
        }


        private async void LoadUser()
        {
            string UserID = await SecureStorage.Default.GetAsync("UserID");

            if (UserID == null)
            {
                return;
                // No value for UserID yet
            }

            UserIdEntry = UserID;
        }
        async void StoreUserId(string UserID)
        {
            await SecureStorage.Default.SetAsync("UserID", UserID);
        }

        public async void OnBLE_connectClicked()
        {
            //await Shell.Current.GoToAsync(nameof(PopUpBLE), true);
            Shell.Current.CurrentPage.ShowPopup(new PopUpBLE(new PopUpBLEViewModel(ble, rawDataSender)));
            //Shell.Current.CurrentPage.ShowPopup(new PopUpBLE());


        }

    }
}
