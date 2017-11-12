using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.PaymentHelper
{
    public class PaymentConfigHelper
    {
        //零时尚应用ID
        //public static string WxAppId = "wxe3a68f19dc3d3f36";
        public static string Wx_0Fashion_AppId = "wx844834db9123c470";
        public static string Wx_XD_AppId = "wx98affa86120fc304";

        //微信支付商户号
        //public static string WxMCHID = "1346730701";
        public static string Wx_0Fashion_MCHID = "1484178112";
        public static string Wx_XD_MCHID = "1377696102";

        //服务端Ip 扫码支付用
        public static string Ip = "221.223.242.98";
        //商户密钥 
        //public static string WxKEY = "5XmRTEs4AjiyKTk8GBLAywl0pJxKTGOp";
        public static string WxKEY = "xXsppC5q9o0VDPIC4vgeub5XTd3xb1rK";

        //接口加密 密钥
        public static string signKey = "nMEu1y88BBJKVy2MO52jsKQGFsdyNISX";
        //public static string signKey = "xXsppC5q9o0VDPIC4vgeub5XTd3xb1rK";

        //微信支付成功回调处理
        //public static string NOTIFY_URL = "https://0-fashion.imwork.net:8012/Finance/ResultNotify/Notify";
        public static string WX_0Fashion_NOTIFY_URL = "https://0-fashion.imwork.net:8012/Finance/ResultNotify/Notify/";
        public static string WX_XD_NOTIFY_URL = "https://api.0-fashion.com/Recharge/ResultNotifyWXHandler/";
    }
}
