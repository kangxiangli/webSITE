using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.RongCloudServer
{
    public abstract class MsgBase
    {
        public abstract string GetMsgType();

        public UserInfo user { get; set; }
    }
    public class PlainMsg: MsgBase
    {
        public override string GetMsgType()
        {
            return "RC:TxtMsg";
        }

        public string content { get; set; }
        public string extra { get; set; }

    }


    public class InfoMsg : MsgBase
    {
        public string message { get; set; }
       
        public string extra { get; set; }

        public override string GetMsgType()
        {
            return "RC:InfoNtf";
        }
    }

}
