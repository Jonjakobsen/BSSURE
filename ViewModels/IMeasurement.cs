using Bssure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.ViewModels
{
    public interface IMeasurement
    {
        event EventHandler<StartMeasurementEventArgs> MeasurementStartedEvent;
    }
}
