using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Models
{
    public class M_WeChatNotify
    {
        /// <summary>
        /// 返回状态码	
        /// </summary>
        public string return_code { get; set; }

        /// <summary>
        /// 返回信息	
        /// </summary>
        public string return_msg { get;set;}

        public string appid { get; set; }


    }
}