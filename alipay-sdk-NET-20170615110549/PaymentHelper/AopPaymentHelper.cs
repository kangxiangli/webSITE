

using Aop.Api.Request;
using Aop.Api.Response;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Aop.Api.PaymentHelper
{
    public static class AopPaymentHelper
    {
        public static string UnifiedOrder(AopObject model)
        {
            #region App支付
            IAopClient client = new DefaultAopClient(AlipayPaymentConfigHelper.Server_Url, AlipayPaymentConfigHelper.App_Id, AlipayPaymentConfigHelper.PrivateKeyPem, AlipayPaymentConfigHelper.Format, AlipayPaymentConfigHelper.Version, AlipayPaymentConfigHelper.Sign_Type, AlipayPaymentConfigHelper.PublicKeyPem, AlipayPaymentConfigHelper.Charset, false, AlipayPaymentConfigHelper.Notify_Url);
            //实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：alipay.trade.app.pay
            AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。

            request.SetBizModel(model);
            //这里和普通的接口调用不同，使用的是sdkExecute
            AlipayTradeAppPayResponse response = client.SdkExecute(request);
            //HttpUtility.HtmlEncode是为了输出到页面时防止被浏览器将关键参数html转义，实际打印到日志以及http传输不会有这个问题
            #endregion

            #region 电脑网站支付
            //Random rad = new Random();
            //string NoId = rad.Next(1000, 9999).ToString() + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)] + rad.Next(1000, 9999) + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)];
            //IAopClient client = new DefaultAopClient(AlipayPaymentConfigHelper.Server_Url, AlipayPaymentConfigHelper.App_Id, AlipayPaymentConfigHelper.PrivateKeyPem, AlipayPaymentConfigHelper.Format, AlipayPaymentConfigHelper.Version, AlipayPaymentConfigHelper.Sign_Type, AlipayPaymentConfigHelper.PublicKeyPem, AlipayPaymentConfigHelper.Charset, false);
            //AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
            //request.BizContent = "{" +
            //"    \"body\":\"Iphone6 16G\"," +
            //"    \"subject\":\"Iphone6 16G\"," +
            //"    \"out_trade_no\":\"" + NoId + "\"," +
            //"    \"total_amount\":88.88," +
            //"    \"product_code\":\"FAST_INSTANT_TRADE_PAY\"" +
            //"  }";
            //AlipayTradePagePayResponse response = client.pageExecute(request);
            //string form = response.Body;
            //Response.Write(form);
            #endregion

            return response.Body.ToString();
        }

        #region 获取支付宝返回的参数
        /// <summary>
        /// 获取支付宝返回的参数
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetRequestPost(HttpRequestBase request)
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = request.Form;

            String[] requestItem = coll.AllKeys;
            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], request.Form[requestItem[i]]);
            }

            return sArray;
        }
        #endregion
    }
}
