using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.RongCloudServer
{

    public class NtfMsg : MsgBase
    {

        public string operation { get; set; }
        public string sourceUserId { get; set; }
        public string targetUserId { get; set; }
        public string message { get; set; }
        public string extra { get; set; }



        public override string GetMsgType()
        {
            return "RC:ContactNtf";
        }
    }


}
