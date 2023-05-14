using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.Constants
{
    public static class Topics
    {

        private const string pre = "Dev/";
        public const string

            Topic_Status_CSSURE = pre+"ECG/Status/CSSURE",
            Topic_Status_BSSURE = pre+"ECG/Status/BSSURE",


            Topic_UserData = pre+"ECG/Userdata",
            Topic_Series_FromBSSURE = pre+"ECG/Series/BSSURE2CSSURE",
            Topic_Series_TempToBSSURE = pre+"ECG/Temp/ToBSSURE";
    }
}
