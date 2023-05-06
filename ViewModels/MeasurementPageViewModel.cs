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


        private string _StartBtnText;
        public string StartBtnText
        {
            get => _StartBtnText;
            set => SetProperty(ref _StartBtnText, value);
        }


        private string _CSIEntry;
        public string CSI
        {
            get => _CSIEntry;
            set => SetProperty(ref _CSIEntry, value);
        }



        private string _MODCsiEntry;

        public string MODCsi
        {
            get => _MODCsiEntry;
            set => SetProperty(ref _MODCsiEntry, value);
        }


        private string _RMSEntry;

        public string RMS
        {
            get => _RMSEntry;
            set => SetProperty(ref _RMSEntry, value);
        }
        public IMQTTService MQTTService { get; }
        #endregion

        public MeasurementPageViewModel(IMQTTService mQTTService)
        {
            System.Threading.Thread.Sleep(100);
            OnSetDefaultValuesClicked();
            LoadUserValues();
            StartBtnText = StartText;
            StartMeasurementCommand = new RelayCommand(Onstart_measurementClicked);
            SetDefaultValuesCommand = new RelayCommand(OnSetDefaultValuesClicked);
            BackToMainpageCommand = new RelayCommand(OnHomeClicked);
            MQTTService = mQTTService;
        }

        private void OnSetDefaultValuesClicked()
        {
            MODCsi = "1000000";
            CSI = "1000000";
            RMS = "10000";
        }

        private async void LoadUserValues()
        {
            string MODCsiFromStorage = await SecureStorage.Default.GetAsync("MODCsi");

            if (MODCsiFromStorage == null)
            {
                // No value is associated with the key "MODCsi"
                MODCsi = "1000000"; // Set to default instead

            }
            else MODCsi = MODCsiFromStorage;

            string CSIFromStorage = await SecureStorage.Default.GetAsync("CSI");

            if (CSIFromStorage == null)
            {
                // No value is associated with the key "CSI"
                CSI = "1000000"; // Set to default instead

            }
            else CSI = CSIFromStorage;

            string RMSFromStorage = await SecureStorage.Default.GetAsync("RMS");

            if (RMSFromStorage == null)
            {
                // No value is associated with the key "RMS"
                RMS = "10000"; // Set to default instead
            }
            else RMS = RMSFromStorage;
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

        public async void MODCsi_TextChanged(object sender, TextChangedEventArgs e)
        {
            string MODCsi = ((Entry)sender).Text;
            if (MODCsi == "1000000")
            {
                return; // Don't save if default
            }
            if (MODCsi == "")
            {
                return;
            }
            await SecureStorage.Default.SetAsync("MODCsi", MODCsi);

        }

        public async void CSIEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string CSI = ((Entry)sender).Text;
            if (CSI == "1000000")
            {
                return; // Don't save if default
            }
            if (CSI == "")
            {
                return;
            }
            await SecureStorage.Default.SetAsync("CSI", CSI);
        }

        public async void RMSEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string RMS = ((Entry)sender).Text;
            if (RMS == "10000")
            {
                return; // Don't save if default
            }
            if (RMS == "")
            {
                return;
            }
            await SecureStorage.Default.SetAsync("RMS", RMS);
        }

        private async void OnHomeClicked()
        {
            await Shell.Current.GoToAsync(nameof(MainPage), true);
        }

    }
}
