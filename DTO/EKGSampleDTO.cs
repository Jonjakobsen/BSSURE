using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.DTO
{
    //make this class an observable by inheriting from the INotifyPropertyChanged interface
    public class EKGSampleDTO
    {
        public sbyte[] RawBytes { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string PatientId { get; set; }



    }
}
