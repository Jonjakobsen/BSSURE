﻿using Bssure.CortriumDevice;
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

using Bssure.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Bssure.Events;

namespace Bssure.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {

        //public IBLEservice ble { get; private set; } //This is the service that is injected into the viewmodel, that handles Plugin.Ble
        public IMQTTService mqttService { get; }

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

        public ObservableCollection<DeviceCandidate> ListOfDeviceCandidates
        {
            get { return _listOfDeviceCandidates; }
            set
            {
                _listOfDeviceCandidates = value;
            }
        } //This is the list of devices that is shown in the UI
        private ObservableCollection<DeviceCandidate> _listOfDeviceCandidates = new ObservableCollection<DeviceCandidate>(); //This is the list of devices that is shown in the UI
        public IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; } //kræver knap
        public IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; } //kræver knap
        public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
        public DeviceCandidate BleDevice { get; set; } //This is the device that is selected in the UI
        public IService EKGservice { get; private set; }
        public ICharacteristic EKGCharacteristic { get; private set; }

        private ObservableCollection<EKGSampleDTO> _ekgSamples = new ObservableCollection<EKGSampleDTO>();
        public ObservableCollection<EKGSampleDTO> EKGSamples { get { return _ekgSamples; } set { _ekgSamples = value; } }


        private string textnotScanning = "Scan for nearby devices";
        private string textScanning = "Scanning";

        private string bleNotScaning = "Scan for nearby devices";

        public string BleNotScaning
        {
            get => bleNotScaning;
            set => SetProperty(ref bleNotScaning, value);
        }

        private bool bleConnceted;
        private readonly IDecoder decoder;

        public bool BleConnected
        {
            get => bleConnceted;
            set => SetProperty(ref bleConnceted, value);
        }


        public BLEservice BLEservice { get; set; }
        private bool _measurementIsStarted { get; set; }

        public MainPageViewModel(BLEservice ble, IMQTTService mQTTService, IDecoder decoder, IMeasurement measurement) //Dependency injection of the BLEservice is necessary in all viewmodel classes. Passed globally from singleton in mauiprogram.cs
        {

            try
            {
                mqttService = mQTTService;
                this.decoder = decoder;
                BLEDisconnectCommand = new RelayCommand(OnBLE_disconnectClicked);
                SubmitUserIDCommand = new RelayCommand(OnSubmitClicked);
                LoadUser();


                BLEservice = ble;
                ListOfDeviceCandidates = new ObservableCollection<DeviceCandidate>();
                //Her bindes kommandoer til CommunityMVVM toolkit Asyncrelay, så de kan kaldes asynkront
                ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand<DeviceCandidate>(async (deviceCandidate) => await ConnectToDeviceCandidateAsync(deviceCandidate));
                ScanNearbyDevicesAsyncCommand = new AsyncRelayCommand(ScanDevicesAsync);
                CheckBluetoothAvailabilityAsyncCommand = new AsyncRelayCommand(CheckBluetoothAvailabilityAsync);
                measurement.MeasurementStartedEvent += HandleStartMeasurementEvent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error during Main Page View Model Creation: " + ex);
            }
        }

        private async void OnBLE_disconnectClicked()
        {
            BleConnected = false;

            if (BLEservice.DeviceInterface == null)
            {
                await BLEservice.ShowToastAsync("No device connected", $"You are not connected to a device");
                return;
            }

            if (!BLEservice.bleInterface.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn bluetooth on and try again.", "OK");
                return;
            }

            if (BLEservice.AdapterInterface.IsScanning)
            {
                await BLEservice.ShowToastAsync("Scan in progress", $"Bluetooth adapter is scanning. Try again.");
                return;
            }

            if (BLEservice.DeviceInterface.State == DeviceState.Disconnected)
            {
                await BLEservice.ShowToastAsync("Bluetooth", $"{BLEservice.DeviceInterface.Name} is already disconnected.");
                return;
            }

            try
            {

                mqttService.StopSending();
                await BLEservice.AdapterInterface.DisconnectDeviceAsync(BLEservice.DeviceInterface);


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to disconnect from {BLEservice.DeviceInterface.Name} {BLEservice.DeviceInterface.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{BLEservice.DeviceInterface.Name}", $"Unable to disconnect from {BLEservice.DeviceInterface.Name}.", "OK");
            }
            finally
            {
                byte[] stopByte = new byte[1] { 0x00 }; //0x00 is the stop byte, with the value 0
                DateTime stopTime = DateTimeOffset.Now.LocalDateTime;
                BLEservice.DeviceInterface?.Dispose();
                BLEservice.DeviceInterface = null;
                //await Shell.Current.GoToAsync("//MainPage", true);
            }
        }

        private async void OnSubmitClicked()
        {
            if (UserIdEntry == "" || UserIdEntry == null)
            {
                await Application.Current.MainPage.DisplayAlert("No user ID", $"Input user ID to continue", "OK");

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
            if (CaretakerEmail == null || CaretakerEmail == "")
                CaretakerEmail = "";
            await SecureStorage.Default.SetAsync("Email", CaretakerEmail);
            await SecureStorage.Default.SetAsync("UserID", UserID);
        }

        async Task ScanDevicesAsync()
        {
            await Task.Run(() =>
            {
                Thread thread1 = new Thread(ScanThreaded);
                thread1.Start();
            });
        }

        public async void ScanThreaded()
        {

            try
            {
                BleNotScaning = textScanning;
                if (!BLEservice.bleInterface.IsAvailable)
                {
                    Debug.WriteLine($"Bluetooth is missing.");
                    await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
                    return;
                }

#if ANDROID
            PermissionStatus permissionStatus = await BLEservice.CheckBluetoothPermissions();
            if (permissionStatus != PermissionStatus.Granted)
            {
                permissionStatus = await BLEservice.RequestBluetoothPermissions();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth LE permissions", $"Bluetooth LE permissions are not granted.", "OK");
                    return;
                }
            }
#elif IOS
#elif WINDOWS
#endif

                try
                {
                    if (!BLEservice.bleInterface.IsOn)
                    {
                        await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                        return;
                    }



                    List<DeviceCandidate> newlyFoundDevices = await BLEservice.ScanForDevicesAsync();

                    if (newlyFoundDevices.Count == 0)
                    {
                        await BLEservice.ShowToastAsync("BLE Error", $"Unable to find nearby Bluetooth LE devices. Try again.");
                    }

                    if (ListOfDeviceCandidates.Count > 0) // clear the old global list
                    {
                        ListOfDeviceCandidates.Clear();
                    }


                    foreach (var deviceCandidate in newlyFoundDevices) //Fill the global list with newly found devices
                    {
                        ListOfDeviceCandidates.Add(deviceCandidate); //add the found devices to the global list for the viewmodel
                    }
                    //TODO: Den connecter direkte til det første device den finder, bør laves om så man selv skal udvælge det
                    //if (ListOfDeviceCandidates.Count >= 1)
                    //{
                    //    await ConnectToDeviceCandidateAsync(ListOfDeviceCandidates.First());
                    //}


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to get nearby Bluetooth LE devices: {ex.Message}");
                    await Shell.Current.DisplayAlert($"Unable to get nearby Bluetooth LE devices", $"{ex.Message}.", "OK");
                }
                BleNotScaning = textnotScanning;
            }
            catch (Exception ex2)
            {
                Debug.WriteLine($"Bssure Error while getting Bluetooth LE devices: {ex2.Message}");
            }
        }
        async Task CheckBluetoothAvailabilityAsync()
        {


            try
            {
                if (!BLEservice.bleInterface.IsAvailable)
                {
                    Debug.WriteLine($"Error: Bluetooth is missing.");
                    await Shell.Current.DisplayAlert($"Bluetooth", $"Bluetooth is missing.", "OK");
                    return;
                }

                if (BLEservice.bleInterface.IsOn)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is on", $"You are good to go.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to check Bluetooth availability: {ex.Message}");
                await Shell.Current.DisplayAlert($"Unable to check Bluetooth availability", $"{ex.Message}.", "OK");
            }
        }
        private async Task ConnectToDeviceCandidateAsync(DeviceCandidate deviceCandidate)
        {

            try
            {
                BLEservice.BleDevice = deviceCandidate;


                if (!BLEservice.bleInterface.IsOn)
                {
                    await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                    return;
                }

                if (BLEservice.AdapterInterface.IsScanning)
                {
                    await BLEservice.ShowToastAsync("Bluetooth adapter is scanning.", $"Try again.");
                    return;
                }

                try
                {

                    if (BLEservice.DeviceInterface != null)
                    {
                        if (BLEservice.DeviceInterface.State == DeviceState.Connected)
                        {
                            if (BLEservice.DeviceInterface.Id.Equals(BLEservice.BleDevice.Id))
                            {
                                await BLEservice.ShowToastAsync("Connection error.", $"{BLEservice.DeviceInterface.Name} is already connected.");
                                return;
                            }

                            if (BLEservice.BleDevice != null)
                            {
                                #region another device
                                if (!BLEservice.DeviceInterface.Id.Equals(BLEservice.BleDevice.Id))
                                {
                                    Debug.WriteLine($"Disconnected: {BLEservice.BleDevice.Name}");
                                    await DisconnectFromDeviceAsync();
                                    await BLEservice.ShowToastAsync("Succes.", $"{BLEservice.DeviceInterface.Name} has been disconnected.");
                                    BleConnected = false;
                                }
                                #endregion another device
                            }
                        }
                    }

                    BLEservice.DeviceInterface = await BLEservice.AdapterInterface.ConnectToKnownDeviceAsync(BLEservice.BleDevice.Id);

                    if (BLEservice.DeviceInterface.State == DeviceState.Connected)
                    {
                        EKGservice = await BLEservice.DeviceInterface.GetServiceAsync(CortriumUUIDs.BLE_SERVICE_UUID_C3TESTER[0]);
                        if (EKGservice != null)
                        {
                            EKGCharacteristic = await EKGservice.GetCharacteristicAsync(CortriumUUIDs.BLE_CHARACTERISTIC_UUID_Rx[0]); //0 is because of array, want the first one eventhough there is only one
                            if (EKGCharacteristic != null)
                            {
                                if (EKGCharacteristic.CanUpdate)
                                {
                                    BleConnected = true;
                                    Debug.WriteLine($"Found service: {EKGservice.Device.Name}");

                                    #region save device id to storage
                                    await SecureStorage.Default.SetAsync("device_name", $"{BLEservice.DeviceInterface.Name}");
                                    await SecureStorage.Default.SetAsync("device_id", $"{BLEservice.DeviceInterface.Id}");
                                    #endregion save device id to storage

                                    EKGCharacteristic.ValueUpdated += ECGMeasurementCharacteristic_ValueUpdated;
                                    await EKGCharacteristic.StartUpdatesAsync();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to connect to {BLEservice.BleDevice.Name} {BLEservice.BleDevice.Id}: {ex.Message}.");
                    await Shell.Current.DisplayAlert($"{BLEservice.BleDevice.Name}", $"Unable to connect to {BLEservice.BleDevice.Name}.", "OK");
                }
            }
            catch (Exception ex2)
            {
                Debug.WriteLine($"Error during: ConnectToDeviceCandidateAsync: {ex2.Message}.");
                //await Shell.Current.DisplayAlert($"{BLEservice.BleDevice.Name}", $"Unable to connect to {BLEservice.BleDevice.Name}.", "OK");
            }

        }
        private async Task DisconnectFromDeviceAsync()
        {


            if (BLEservice.DeviceInterface == null)
            {
                await BLEservice.ShowToastAsync("No Device Error", $"Nothing to do.");
                return;
            }

            if (!BLEservice.bleInterface.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                return;
            }

            if (BLEservice.AdapterInterface.IsScanning)
            {
                await BLEservice.ShowToastAsync("Scan in progress", $"Bluetooth adapter is scanning. Try again.");
                return;
            }

            if (BLEservice.DeviceInterface.State == DeviceState.Disconnected)
            {
                await BLEservice.ShowToastAsync("Bluetooth", $"{BLEservice.DeviceInterface.Name} is already disconnected.");
                return;
            }

            try
            {

                await EKGCharacteristic.StopUpdatesAsync();

                await BLEservice.AdapterInterface.DisconnectDeviceAsync(BLEservice.DeviceInterface);

                EKGCharacteristic.ValueUpdated -= ECGMeasurementCharacteristic_ValueUpdated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to disconnect from {BLEservice.DeviceInterface.Name} {BLEservice.DeviceInterface.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{BLEservice.DeviceInterface.Name}", $"Unable to disconnect from {BLEservice.DeviceInterface.Name}.", "OK");
            }
            finally
            {
                byte[] stopByte = new byte[1] { 0x00 }; //0x00 is the stop byte, with the value 0
                DateTime stopTime = DateTimeOffset.Now.LocalDateTime;
                sbyte[] bytessigned = Array.ConvertAll(stopByte, x => unchecked((sbyte)x));
                EKGSamples.Add(new EKGSampleDTO { RawBytes = bytessigned, Timestamp = stopTime });
                BLEservice.DeviceInterface?.Dispose();
                BLEservice.DeviceInterface = null;
                //await Shell.Current.GoToAsync("//MainPage", true);
            }
        }

        private void HandleStartMeasurementEvent(object sender, StartMeasurementEventArgs e)
        {
            _measurementIsStarted = e.MeasurementIsStarted;
        }

        //This is the eventhandler that receives raw samples from the device
        private async void ECGMeasurementCharacteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            try
            {
                if (_measurementIsStarted == false) //start measurement button has not been clicked yet
                {
                    return;
                }
                else //start measurement button is clicked and text should now be "Stop measurement"
                {
                    var bytes = e.Characteristic.Value;//byte array, with raw data to be sent to CSSURE
                    sbyte[] bytessigned = Array.ConvertAll(bytes, x => unchecked((sbyte)x));
                    var time = DateTimeOffset.Now.LocalDateTime;

                    await Task.Run(() => decoder.DecodeBytes(bytessigned));

                    //Add the newest sample to the list
                    EKGSampleDTO item = new EKGSampleDTO { RawBytes = bytessigned, Timestamp = time };
                    EKGSamples.Add(item);

                    _ = sendDataAsync(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("heart rate measurement found " + ex);
            }
        }

        private async Task sendDataAsync(EKGSampleDTO item)
        {

            await Task.Run(() => mqttService.Publish_RawData(item));
        }

    }
}
