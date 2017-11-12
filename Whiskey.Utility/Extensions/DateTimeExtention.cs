using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Whiskey.Utility.Extensions
{
    public static class DateTimeExtention
    {
        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        public static string ToLostTimeStr(this DateTime time)
        {
            var now = DateTime.Now;
            var timespan = now - time;
            var strResult = timespan.Days > 0 ? timespan.Days + "天前" : timespan.Hours > 0 ? timespan.Hours + "小时前" : timespan.Minutes > 0 ? timespan.Minutes + "分钟前" : "刚刚";
            return strResult;
        }
    }
}