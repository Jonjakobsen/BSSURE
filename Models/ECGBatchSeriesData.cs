﻿namespace Bssure.Models
{
    public class ECGBatchSeriesData
    {
        public string PatientID { get; set; }
        public List<long> TimeStamp { get; set; }
        public List<int[]> ECGChannel1 { get; set; }
        public List<int[]> ECGChannel2 { get; set; }
        public List<int[]> ECGChannel3 { get; set; }
        public int Samples { get; set; }
    }
}
