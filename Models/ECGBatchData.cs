﻿namespace Bssure.Models
{
    public class ECGBatchData
    {
        public string PatientID { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public int[] ECGChannel1 { get; set; }
        public int[] ECGChannel2 { get; set; }
        public int[] ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
