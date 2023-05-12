using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bssure.DTO
{
    public class UserDataDTO
    {
        public string UserId { get; set; }
        public float[] CSINormMax { get; set; }
        public float[] ModCSINormMax { get; set; }
        public string[] Emails { get; set; }

    }
}
