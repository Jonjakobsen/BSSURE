using Bssure.Models;

namespace Bssure.Events
{
    public class ECGDataReceivedEventArgs : EventArgs
    {
        public ECGBatchData ECGBatch { get; set; }
    }
}
