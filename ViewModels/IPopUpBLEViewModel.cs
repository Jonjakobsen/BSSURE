using Bssure.CortriumDevice;
using Bssure.DTO;
using Bssure.Services;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;

namespace Bssure.ViewModels
{
    public interface IPopUpBLEViewModel
    {
        DeviceCandidate BleDevice { get; set; }
        BLEservice BLEservice { get; set; }
        IAsyncRelayCommand CheckBluetoothAvailabilityAsyncCommand { get; }
        IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
        ICharacteristic EKGCharacteristic { get; }
        ObservableCollection<EKGSampleDTO> EKGSamples { get; set; }
        IService EKGservice { get; }
        ObservableCollection<DeviceCandidate> ListOfDeviceCandidates { get; set; }
        IRawDataService RawDataSender { get; }
        IAsyncRelayCommand ScanNearbyDevicesAsyncCommand { get; }
    }
}