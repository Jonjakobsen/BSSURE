using Bssure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.Events
{
    public class StartMeasurementEventArgs : EventArgs
    {
        public bool MeasurementIsStarted { get; set; }
    }
}

