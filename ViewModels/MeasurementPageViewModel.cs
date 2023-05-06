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

        private string _CSI30;
        public string CSI30
        {
            get => _CSI30;
            set
            {
                Changed = true;
                SetProperty(ref _CSI30, value);
            }
        }
        private string _CSI50;
        public string CSI50
        {
            get => _CSI50;
            set
            {
                Changed = true;
                SetProperty(ref _CSI50, value);
            }
        }
        private string _CSI100;
        public string CSI100
        {
            get => _CSI100;
            set
            {

                Changed = true;
                SetProperty(ref _CSI100, value);
            }
        }

        private string _ModCSI30;
        public string ModCSI30
        {
            get => _ModCSI30;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI30, value);
            }

        }
        private string _ModCSI50;
        public string ModCSI50
        {
            get => _ModCSI50;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI50, value);
            }
        }
        private string _ModCSI100;
        public string ModCSI100
        {
            get => _ModCSI100;
            set
            {
                Changed = true;
                SetProperty(ref _ModCSI100, value);
            }
        }


        private string _RMSEntry;

        public string RMS
        {
            get => _RMSEntry;
            set
            {
                Changed = true;
                SetProperty(ref _RMSEntry, value);
            }
        }
        public IMQTTService MQTTService { get; }
        #endregion

        public MeasurementPageViewModel(IMQTTService mQTTService)
        {
            System.Threading.Thread.Sleep(100);
            OnSetDefaultValuesClicked();
            LoadUserValues();
            StartBtnText = StartText;

            SaveAllParametersCommand = new RelayCommand(SaveUserValues);
            StartMeasurementCommand = new RelayCommand(Onstart_measurementClicked);
            SetDefaultValuesCommand = new RelayCommand(OnSetDefaultValuesClicked);
            BackToMainpageCommand = new RelayCommand(OnHomeClicked);
            MQTTService = mQTTService;
        }

        private void OnSetDefaultValuesClicked()
        {
            ModCSI30 = "1000000";
            ModCSI50 = "1000000";
            ModCSI100 = "1000000";
            CSI30 = "1000000";
            CSI50 = "1000000";
            CSI100 = "1000000";
            RMS = "10000";
        }

        private async void LoadUserValues()
        {
            try
            {

                string ModCSI30FromStorage = await SecureStorage.Default.GetAsync("ModCSI30");
                string ModCSI50FromStorage = await SecureStorage.Default.GetAsync("ModCSI50");
                string ModCSI100FromStorage = await SecureStorage.Default.GetAsync("ModCSI100");

                if (ModCSI30FromStorage == null)
                {
                    // No value is associated with the key "MODCsi"
                    ModCSI30 = "1000000"; // Set to default instead
                    ModCSI50 = "1000000"; // Set to default instead
                    ModCSI100 = "1000000"; // Set to default instead

                }
                else
                {
                    ModCSI30 = ModCSI30FromStorage;
                    ModCSI50 = ModCSI50FromStorage;
                    ModCSI100 = ModCSI100FromStorage;
                }

                string CSI30FromStorage = await SecureStorage.Default.GetAsync("CSI30");
                string CSI50FromStorage = await SecureStorage.Default.GetAsync("CSI50");
                string CSI100FromStorage = await SecureStorage.Default.GetAsync("CSI100");

                if (ModCSI30FromStorage == null)
                {
                    // No value is associated with the key "MODCsi"
                    CSI30 = "1000000"; // Set to default instead
                    CSI50 = "1000000"; // Set to default instead
                    CSI100 = "1000000"; // Set to default instead

                }
                else
                {
                    CSI30 = CSI30FromStorage;
                    CSI50 = CSI50FromStorage;
                    CSI100 = CSI100FromStorage;
                }

                string RMSFromStorage = await SecureStorage.Default.GetAsync("RMS");

                if (RMSFromStorage == null)
                {
                    // No value is associated with the key "RMS"
                    RMS = "10000"; // Set to default instead
                }
                else RMS = RMSFromStorage;
                Changed = false;
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
                await SecureStorage.Default.SetAsync("ModCSI30", ModCSI30);
                await SecureStorage.Default.SetAsync("ModCSI50", ModCSI50);
                await SecureStorage.Default.SetAsync("ModCSI100", ModCSI100);
                await SecureStorage.Default.SetAsync("CSI30", CSI30);
                await SecureStorage.Default.SetAsync("CSI50", CSI50);
                await SecureStorage.Default.SetAsync("CSI100", CSI100);
                await SecureStorage.Default.SetAsync("RMS", RMS);

                Changed = false;
                await Application.Current.MainPage.DisplayAlert("Saved", "You have saved your parameters", "OK");
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Save ERROR", "Could not save the parameters", "OK");

            }
        }

        private void Onstart_measurementClicked()
        {

            if (StartBtnText == StartText)
            {
                StartBtnText = StopText;
                //Todo:Her startes målingen
                MQTTService.StartSending();
            }
            else
            {
                StartBtnText = StartText;
                //Todo:Her stoppes målingen
                MQTTService.StopSending();
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
