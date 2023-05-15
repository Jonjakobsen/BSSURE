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
using Bssure.Events;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Bssure.ViewModels
{
    public class ECGGraph //this is not the graph itself, but the data that is used to draw the graph. The x and y, that we bind to in xaml.
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
        public event EventHandler<StartMeasurementEventArgs> MeasurementStartedEvent;

        private List<int> bufferForGraph = new List<int>();

        private ObservableCollection<ECGGraph> _ecgSamples;
        public ObservableCollection<ECGGraph> ECGSamples
        {
            get => _ecgSamples;
            set => SetProperty(ref _ecgSamples, value);
        }

        private string _ecgGraphTitle;
        public string ECGGraphTitle
        {
            get => _ecgGraphTitle;
            set => SetProperty(ref _ecgGraphTitle, value);
        }

        #endregion

        public MeasurementPageViewModel(IMQTTService mQTTService, BLEservice ble, IDecoder decoder)
        {
            System.Threading.Thread.Sleep(100);
            mqttService = mQTTService;
            bleService = ble;
            this.decoder = decoder;
            OnSetDefaultValuesClicked();
            LoadUserValues();
            StartBtnText = StartText;
            ECGSamples = new ObservableCollection<ECGGraph>();

            SaveAllParametersCommand = new RelayCommand(SaveUserValues);
            SetDefaultValuesCommand = new RelayCommand(OnSetDefaultValuesClicked);
            BackToMainpageCommand = new RelayCommand(OnHomeClicked);
            StartMeasurementCommand = new RelayCommand(OnStartMeasurementClicked);
            decoder.ECGDataReceivedEvent += HandleECGDataReceivedEvent;
        }

        public MeasurementPageViewModel()
        {

        }

        string UserID = "Unknown";
        private async Task OnSendPersonalMetadataAsync()
        {
            float[] csi = new float[] { CSI30, CSI50, CSI100 };
            float[] modcsi = new float[] { ModCSI30, ModCSI50, ModCSI100 };

            UserID = await SecureStorage.Default.GetAsync("UserID");
            string email = await SecureStorage.Default.GetAsync("Email");

            UserDataDTO userDataDTO = new UserDataDTO { CSINormMax = csi, ModCSINormMax = modcsi, UserId = UserID, Emails = new string[] { email } };

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

        protected virtual void OnStartMeasurementEvent(StartMeasurementEventArgs e) //when measurement button is clicked event is fired
        {
            MeasurementStartedEvent?.Invoke(this, e);
        }

        object _lockECGSamples = new object();
        object _lockECG = new object();

        private void HandleECGDataReceivedEvent(object sender, ECGDataReceivedEventArgs e)
        {
            lock (_lockECG)
            {

                ECGGraphTitle = nameof(e.ECGBatch.ECGChannel1).ToString();
                try
                {
                    //foreach (var sample in e.ECGBatch.ECGChannel1) //loop through the received ECG Batch
                    //{
                    //    bufferForGraph.Add(sample);
                    //}
                    try
                    {
                        bufferForGraph.Add(e.ECGBatch.ECGChannel1[0]);
                    }
                    catch (Exception bufferEx)
                    {
                        Debug.WriteLine(bufferEx.Message + "Adding to buffer failed in MeasurementPageViewModel");
                    }

                    //buffer for 3 seconds, so that graph isnt overpopulated
                    if (bufferForGraph.Count() <= 63) //check if there is 756 samples in the buffer ECGSamples
                    {
                        return;
                    }
                    else
                    {
                        lock (_lockECGSamples)
                        {
                            ECGSamples.Clear();
                        }
                        foreach (int ecgSample in bufferForGraph)
                        {
                            ECGSamples.Add(new ECGGraph() { X = DateTime.Now, Y = (int)ecgSample }); //Take one of the 12 samples to not overpopulate graph
                        }
                        bufferForGraph.Clear(); //Clear the collection, to get a new buffer for next 3 sec.
                    }
                }
                catch (InvalidCastException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        async private void OnStartMeasurementClicked()
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
                    OnStartMeasurementEvent(new StartMeasurementEventArgs { MeasurementIsStarted = true });
                    UserID = await SecureStorage.Default.GetAsync("UserID");
                    _ = OnSendPersonalMetadataAsync();
                    StartBtnText = StopText;
                    mqttService.StartSending(UserID);
                }
            }
            else
            {
                StartBtnText = StartText;
                //Todo:Her stoppes målingen
                OnStartMeasurementEvent(new StartMeasurementEventArgs { MeasurementIsStarted = false });
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
