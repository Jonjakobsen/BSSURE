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
using System.Collections.ObjectModel;
using Bssure.DecodingBytes;
using Bssure.Models;
using System.Diagnostics;
using Bssure.Events;
using System.Diagnostics.Metrics;

namespace Bssure.ViewModels
{
    public class ECGGraph
    {
        public DateTime X { get; set; }
        public int Y { get; set; }
    }

    public partial class MeasurementPageViewModel : ObservableObject, IMeasurement
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
        private readonly IDecoder decoder;
        public Thread graphThread { get; set; }

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

        public IMQTTService MQTTService { get; }


        private ObservableCollection<ECGGraph> _ecgSamples;
        public ObservableCollection<ECGGraph> ECGSamples
        {
            get => _ecgSamples;
            set => SetProperty(ref _ecgSamples, value);
        }

        #endregion

        string UserID = "Unknown";
        private bool measurementStarted;

        public event EventHandler<StartMeasurementEventArgs> MeasurementStartedEvent;

        public MeasurementPageViewModel(IMQTTService mQTTService, BLEservice bleService, IDecoder decoder)
        {
            System.Threading.Thread.Sleep(100);
            OnSetDefaultValuesClicked();
            LoadUserValues();
            StartBtnText = StartText;
            ECGSamples = new ObservableCollection<ECGGraph>();

            SaveAllParametersCommand = new RelayCommand(SaveUserValues);
            StartMeasurementCommand = new RelayCommand(OnstartMeasurementClicked);
            SetDefaultValuesCommand = new RelayCommand(OnSetDefaultValuesClicked);
            BackToMainpageCommand = new RelayCommand(OnHomeClicked);
            MQTTService = mQTTService;
            this.bleService = bleService;
            this.decoder = decoder;

            measurementStarted = false; // set intial state of the measurement event

            decoder.ECGDataReceivedEvent += HandleECGDataReceivedEvent;

            //graphThread = new Thread(Dequeue);
            //graphThread.IsBackground = true;
            //graphThread.Start(); //moved start to characteristic value updated in popupbleviewmodel
        }

        //default constructor needed 
        public MeasurementPageViewModel()
        {

        }

        protected virtual void OnStartMeasurmentClicked(StartMeasurementEventArgs e) //when measurement button is clicked event is fired
        {
            MeasurementStartedEvent?.Invoke(this, e);
        }

        private void HandleECGDataReceivedEvent(object sender, ECGDataReceivedEventArgs e)
        {
            try
            { 
                if (ECGSamples.Count % 21 == 0)
                {
                    ECGSamples.Add(new ECGGraph() { X = DateTime.Now, Y = e.ECGBatch.ECGChannel1[0] });
                }
            }
            catch (InvalidCastException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //if (ECGSamples.Count % 252 == 0)
            //{
            //    ECGSamples.Clear();
            //}
        }

        private void Dequeue()
        {
            //while (true) //TODO: make another flag
            //{
            //    Thread.Sleep(1000);
            //    if (decoder.ECGBatchDataQueue.TryTake(out ECGBatchData item, 1000))
            //    {
            //        try
            //        {
            //            //foreach (int sample in item.ECGChannel1)
            //            //{
            //            ECGSamples.Add(new ECGGraph() { X = DateTime.Now, Y = item.ECGChannel1[0] });
            //            //}
            //        }
            //        catch (InvalidCastException e)
            //        {
            //            Debug.WriteLine(e.Message);
            //        }

            //        if (ECGSamples.Count % 252 == 0)
            //        {
            //            ECGSamples.Clear();
            //            // System.InvalidOperationException: 'Cannot change ObservableCollection during a CollectionChanged event.'
            //        }
            //    }
            //}
        }


        private async Task OnSendPersonalMetadataAsync()
        {
            float[] csi = new float[] { CSI30, CSI50, CSI100 };
            float[] modcsi = new float[] { ModCSI30, ModCSI50, ModCSI100 };

            UserID = await SecureStorage.Default.GetAsync("UserID");

            UserDataDTO userDataDTO = new UserDataDTO { CSINormMax = csi, ModCSINormMax = modcsi, UserId = UserID };

            MQTTService.PublishMetaData(userDataDTO);
        }

        private void OnSetDefaultValuesClicked()
        {
            ModCSI30 = 1000000;
            ModCSI50 = 1000000;
            ModCSI100 = 1000000;
            CSI30 = 1000000;
            CSI50 = 1000000;
            CSI100 = 1000000;
            RMS = 10000;
        }

        private async void LoadUserValues()
        {
            try
            {
                string temp = await SecureStorage.Default.GetAsync("ModCSI30"); // if one is not saved, the others arent either
                if (temp == null || temp == "") // No value is associated with the keys
                {
                    ModCSI30 = 1000000; // Set to default instead
                    ModCSI50 = 1000000; // Set to default instead
                    ModCSI100 = 1000000; // Set to default instead


                    CSI30 = 1000000; // Set to default instead
                    CSI50 = 1000000; // Set to default instead
                    CSI100 = 1000000; // Set to default instead

                    RMS = 10000; // Set to default instead

                }
                else
                {
                    float ModCSI30FromStorage = float.Parse(await SecureStorage.Default.GetAsync("ModCSI30"));
                    float ModCSI50FromStorage = float.Parse(await SecureStorage.Default.GetAsync("ModCSI50"));
                    float ModCSI100FromStorage = float.Parse(await SecureStorage.Default.GetAsync("ModCSI100"));
                    ModCSI30 = ModCSI30FromStorage;
                    ModCSI50 = ModCSI50FromStorage;
                    ModCSI100 = ModCSI100FromStorage;
                    float CSI30FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI30"));
                    float CSI50FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI50"));
                    float CSI100FromStorage = float.Parse(await SecureStorage.Default.GetAsync("CSI100"));
                    CSI30 = CSI30FromStorage;
                    CSI50 = CSI50FromStorage;
                    CSI100 = CSI100FromStorage;
                    float RMSFromStorage = float.Parse(await SecureStorage.Default.GetAsync("RMS"));
                    RMS = RMSFromStorage;
                }
                Changed = false;

                _ = OnSendPersonalMetadataAsync();//Send to CSSURE to be used in python csi_calculation and db
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
                await Application.Current.MainPage.DisplayAlert("Saved", "You have saved your parameters", "OK");
            }
            catch (Exception)
            {
                await Application.Current.MainPage.DisplayAlert("Save ERROR", "Could not save the parameters", "OK");

            }
        }

        async private void OnstartMeasurementClicked()
        {

            if (StartBtnText == StartText)
            {
                //Todo:Her startes målingen
                var ble = bleService.DeviceList;
                if (ble == null || ble.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("No device connected", "Go back a connect to a device", "OK");

                }
                else
                {
                    //fire event and start sending
                    OnStartMeasurmentClicked(new StartMeasurementEventArgs { MeasurementIsStarted = true });
                    StartBtnText = StopText;
                    MQTTService.StartSending(UserID);
                }
            }
            else //Her stoppes målingen
            {
                StartBtnText = StartText;
                //send stop meas. event
                OnStartMeasurmentClicked(new StartMeasurementEventArgs { MeasurementIsStarted = false });
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
