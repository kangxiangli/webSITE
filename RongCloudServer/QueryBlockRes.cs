using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.RongCloudServer
{
    public class QueryBlockRes
    {
        public int code { get; set; }
        public User[] users { get; set; }
    }

    public class User
    {
        public string userId { get; set; }
        public string blockEndTime { get; set; }
    }
}
