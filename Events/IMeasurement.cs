using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.Events
{
    public interface IMeasurement
    {
        //this is the connection point for observers
        event EventHandler<StartMeasurementEvent> measurementStartedEvent;
    }
}
