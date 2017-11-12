using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Net.WeChatPay
{
    /// <summary>
    /// 微信支付参数配置
    /// </summary>
    public static class WeChatPayConfig
    {
        //参数文档 https://pay.weixin.qq.com/wiki/doc/api/app/app.php?chapter=9_1
        static WeChatPayConfig()
        {
            Appid = string.Empty;//
            Mch_id = string.Empty;
            InputCharset = "utf-8";
        }

        /// <summary>
        /// 微信开放平台审核通过的应用APPID length(32)
        /// </summary>
        public static string Appid { get; set; }

        /// <summary>
        /// 微信支付分配的商户号 length(32)
        /// </summary> 
        public static string Mch_id { get; set; }

        /// <summary>
        /// 获取或设置 字符编码格式，当前支持 utf-8
        /// </summary>
        public static string InputCharset { get; set; }
    }
}
