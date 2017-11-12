using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.jpush.api.util;
using Newtonsoft.Json;
namespace Whiskey.jpush.api.schedule
{
    public class Enabled
    {
        [JsonProperty]
        private bool enable;

        public void setEnable(bool enable) { 
            this.enable = enable;  
        }
        public bool getEnable()
        {
            return enable;
        }
    }
}
