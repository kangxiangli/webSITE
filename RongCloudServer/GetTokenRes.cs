using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.RongCloudServer
{
    public class GetTokenRes
    {
        public int code { get; set; }
        public string userId { get; set; }
        public string token { get; set; }
    }
}
