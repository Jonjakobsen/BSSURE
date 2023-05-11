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
        public IMQTTService mqttService { get; }
        public ICommand BLEConnectCommand { get; }

        public ICommand BLEDisconnectCommand { get; }

        public ICommand SubmitUserIDCommand { get; }

        // Her starter viewproperties 

        private string _UserIdEntry;
        public string UserIdEntry
        {
            get => _UserIdEntry;
            set => SetProperty(ref _UserIdEntry, value);
        }

        private string caretakerEmail;

        public string CaretakerEmail
        {
            get => caretakerEmail;
            set => SetProperty(ref caretakerEmail, value);
        }







        public MainPageViewModel(BLEservice bluetoothLEService, IMQTTService mQTTService) //Dependency injection of the BLEservice is necessary in all viewmodel classes. Passed globally from singleton in mauiprogram.cs
        {

            ble = bluetoothLEService;
            mqttService = mQTTService;
            BLEConnectCommand = new RelayCommand(OnBLE_connectClicked);
            BLEDisconnectCommand = new RelayCommand(OnBLE_disconnectClicked);
            SubmitUserIDCommand = new RelayCommand(OnSubmitClicked);
            LoadUser();
        }

        private async void OnBLE_disconnectClicked()
        {


            if (ble.DeviceInterface == null)
            {
                await ble.ShowToastAsync("No Device Error", $"Nothing to do.");
                return;
            }

            if (!ble.bleInterface.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                return;
            }

            if (ble.AdapterInterface.IsScanning)
            {
                await ble.ShowToastAsync("Scan in progress", $"Bluetooth adapter is scanning. Try again.");
                return;
            }

            if (ble.DeviceInterface.State == DeviceState.Disconnected)
            {
                await ble.ShowToastAsync("Bluetooth", $"{ble.DeviceInterface.Name} is already disconnected.");
                return;
            }

            try
            {

                mqttService.StopSending();
                await ble.AdapterInterface.DisconnectDeviceAsync(ble.DeviceInterface);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to disconnect from {ble.DeviceInterface.Name} {ble.DeviceInterface.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{ble.DeviceInterface.Name}", $"Unable to disconnect from {ble.DeviceInterface.Name}.", "OK");
            }
            finally
            {
                byte[] stopByte = new byte[1] { 0x00 }; //0x00 is the stop byte, with the value 0
                DateTime stopTime = DateTimeOffset.Now.LocalDateTime;
                ble.DeviceInterface?.Dispose();
                ble.DeviceInterface = null;
                //await Shell.Current.GoToAsync("//MainPage", true);
            }
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
            string Email = await SecureStorage.Default.GetAsync("Email");

            if (UserID == null)
            {
                return;
                // No value for UserID yet
            }
            UserIdEntry = UserID;
            CaretakerEmail = Email;
        }
        async void StoreUserId(string UserID)
        {

            await SecureStorage.Default.SetAsync("Email", CaretakerEmail);
            await SecureStorage.Default.SetAsync("UserID", UserID);
        }

        public async void OnBLE_connectClicked()
        {
            //await Shell.Current.GoToAsync(nameof(PopUpBLE), true);
            Shell.Current.CurrentPage.ShowPopup(new PopUpBLE(new PopUpBLEViewModel(ble, mqttService)));
            //Shell.Current.CurrentPage.ShowPopup(new PopUpBLE());


        }

    }
}
