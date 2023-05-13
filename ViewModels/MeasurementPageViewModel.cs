using Bssure.Pages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bssure.Services;
using Bssure.DTO;

namespace Bssure.ViewModels
{
    public partial class MeasurementPageViewModel : ObservableObject
    {
        #region Commands

        public ICommand StartMeasurementCommand { get; }

        public ICommand SetDefaultValuesCommand { get; }
        public ICommand BackToMainpageCommand { get; }

        #endregion

        #region Fields  
        private readonly string StartText = "Start measurement";
        private readonly string StopText = "Stop measurement";
        private readonly BLEservice bleService;

        #endregion

        #region Properties

        private bool changed;

        public bool Changed
        {
            get { return changed; }
            set
            {
                //changed = value;
                SetProperty(ref changed, value);
            }
        }


        private string _StartBtnText;
        public string StartBtnText
        {
            get => _StartBtnText;
            set => SetProperty(ref _StartBtnText, value);
        }
        public RelayCommand SaveAllParametersCommand { get; }

        private float _CSI30;
        public float CSI30
        {
            get => _CSI30;
            set
            {
                Changed = true;
                SetProperty(ref _CSI30, value);
            }
        }
        private float _CSI50;
        public float CSI50
        {
            get => _CSI50;
            set
            {
                Changed = true;
                SetProperty(ref _CSI50, value);
            }
        }
        private float _CSI100;
        public float CSI100
        {
            get => _CSI100;
            set
            {

                Changed = true;
                SetProperty(ref _CSI100, value);
            }
        }

        private float _ModCSI30;
        public float ModCSI30
        {
            get => _ModCSI30;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI30, value);
            }

        }
        private float _ModCSI50;
        public float ModCSI50
        {
            get => _ModCSI50;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI50, value);
            }
        }
        private float _ModCSI100;
        public float ModCSI100
        {
            get => _ModCSI100;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI100, value);
            }
        }


        private float _RMSEntry;

        public float RMS
        {
            get => _RMSEntry;
            set
            {
                Changed = true;
                SetProperty(ref _RMSEntry, value);
            }
        }
        public IMQTTService mqttService { get; }
        #endregion

        public MeasurementPageViewModel(IMQTTService mQTTService, BLEservice ble)
        {
            System.Threading.Thread.Sleep(100);
            mqttService = mQTTService;
            bleService = ble;


            OnSetDefaultValuesClicked();
            LoadUserValues();
            StartBtnText = StartText;

            SaveAllParametersCommand = new RelayCommand(SaveUserValues);
            StartMeasurementCommand = new RelayCommand(Onstart_measurementClicked);
            SetDefaultValuesCommand = new RelayCommand(OnSetDefaultValuesClicked);
            BackToMainpageCommand = new RelayCommand(OnHomeClicked);
        }

        string UserID = "Unknown";
        private async Task OnSendPersonalMetadataAsync()
        {
            float[] csi = new float[] { CSI30, CSI50, CSI100 };
            float[] modcsi = new float[] { ModCSI30, ModCSI50, ModCSI100 };

            UserID = await SecureStorage.Default.GetAsync("UserID");
            string email = await SecureStorage.Default.GetAsync("Email");

            UserDataDTO userDataDTO = new UserDataDTO { CSINormMax = csi, ModCSINormMax = modcsi, UserId = UserID , Emails= new string[] { email } };

            mqttService.PublishMetaData(userDataDTO);
        }

        private void OnSetDefaultValuesClicked()
        {

            ModCSI30 = 9074;
            ModCSI50 = 8485;
            ModCSI100 = 8719;
            CSI30 = 15.35f;
            CSI50 = 15.49f;
            CSI100 = 17.31f;
            RMS = 10000;
        }

        private async void LoadUserValues()
        {
            try
            {

                var ModCSI30FromStorage = await SecureStorage.Default.GetAsync("ModCSI30");
                float ModCSI50FromStorage = float.Parse(await SecureStorage.Default.GetAsync("ModCSI50"));
                float ModCSI100FromStorage = float.Parse(await SecureStorage.Default.GetAsync("ModCSI100"));

                if (ModCSI30FromStorage == null)
                {
                    // No value is associated with the key "MODCsi"
                    ModCSI30 = 1000000; // Set to default instead
                    ModCSI50 = 1000000; // Set to default instead
                    ModCSI100 = 1000000; // Set to default instead

                }
                else
                {
                    ModCSI30 = float.Parse(ModCSI30FromStorage);
                    ModCSI50 = ModCSI50FromStorage;
                    ModCSI100 = ModCSI100FromStorage;
                }

                float CSI30FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI30"));
                float CSI50FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI50"));
                float CSI100FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI100"));

                if (ModCSI30FromStorage == null)
                {
                    // No value is associated with the key "MODCsi"
                    CSI30 = 1000000; // Set to default instead
                    CSI50 = 1000000; // Set to default instead
                    CSI100 = 1000000; // Set to default instead

                }
                else
                {
                    CSI30 = CSI30FromStorage;
                    CSI50 = CSI50FromStorage;
                    CSI100 = CSI100FromStorage;
                }

                var RMSFromStorage = await SecureStorage.Default.GetAsync("RMS");

                if (RMSFromStorage == null)
                {
                    // No value is associated with the key "RMS"
                    RMS = 10000; // Set to default instead
                }
                else RMS = float.Parse(RMSFromStorage);
                Changed = false;



                _ = OnSendPersonalMetadataAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("error");
            }
        }

        private async void SaveUserValues()
        {
            try
            {
                await SecureStorage.Default.SetAsync("ModCSI30", ModCSI30.ToString());
                await SecureStorage.Default.SetAsync("ModCSI50", ModCSI50.ToString());
                await SecureStorage.Default.SetAsync("ModCSI100", ModCSI100.ToString());
                await SecureStorage.Default.SetAsync("CSI30", CSI30.ToString());
                await SecureStorage.Default.SetAsync("CSI50", CSI50.ToString());
                await SecureStorage.Default.SetAsync("CSI100", CSI100.ToString());
                await SecureStorage.Default.SetAsync("RMS", RMS.ToString());

                Changed = false;

                _ = OnSendPersonalMetadataAsync();
                await Application.Current.MainPage.DisplayAlert("Saved", "You have saved your parameters", "OK");
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Save ERROR", "Could not save the parameters", "OK");

            }
        }

        async private void Onstart_measurementClicked()
        {

            if (StartBtnText == StartText)
            {
                //Todo:Her startes målingen
                var ble = bleService;
                if (ble.DeviceInterface == null)
                {
                    await Application.Current.MainPage.DisplayAlert("No device connected", "Go back and connect to a device", "OK");

                }
                else
                {

                    UserID = await SecureStorage.Default.GetAsync("UserID");
                    StartBtnText = StopText;
                    mqttService.StartSending(UserID);
                }
            }
            else
            {
                StartBtnText = StartText;
                //Todo:Her stoppes målingen
                mqttService.StopSending();
            }

            SemanticScreenReader.Announce(StartBtnText);
        }




        private async void OnHomeClicked()
        {
            await Shell.Current.GoToAsync(".."); // This command will sent you back to previos page
            //await Shell.Current.GoToAsync(nameof(MainPage), true);
        }

    }
}
