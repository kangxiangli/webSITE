using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Whiskey.jpush.api.util;
using Newtonsoft.Json;

namespace Whiskey.jpush.api.schedule
{
    public class Single
    {
        [JsonProperty]
        private string time;

        public void setTime(string time)
        {
            Preconditions.checkArgument(!String.IsNullOrEmpty(time), "The time must not be empty.");
            Preconditions.checkArgument(StringUtil.IsDateTime(time),"the time is not valid");
            this.time = time;
        }

        public string getTime()
        {
            return this.time;
        }



    }
}
