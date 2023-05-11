using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.Events
{
    public class StartMeasurementEvent : EventArgs
    {
        public bool measurementIsStarted { get; set; }  
    }
}
