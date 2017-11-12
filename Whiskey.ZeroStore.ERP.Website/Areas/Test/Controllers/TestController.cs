using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.PaymentHelper;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Test.Controllers
{
    public class TestController : Controller
    {
        // GET: Test/Test
        public ActionResult Index()
        {
            ViewBag.OrderNo = Request.Cookies["OrderNo"];
            return View();
        }

        public JsonResult TestAPP()
        {
            #region App支付
            //IAopClient client = new DefaultAopClient(AlipayPaymentConfigHelper.Server_Url, AlipayPaymentConfigHelper.App_Id, AlipayPaymentConfigHelper.PrivateKeyPem, AlipayPaymentConfigHelper.Format, AlipayPaymentConfigHelper.Version, AlipayPaymentConfigHelper.Sign_Type, AlipayPaymentConfigHelper.PublicKeyPem, AlipayPaymentConfigHelper.Charset, false);
            ////实例化具体API对应的request类,类名称和接口名称对应,当前调用接口名称如：alipay.trade.app.pay
            //AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            ////SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
            //AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
            //model.Body = "我是测试数据";
            //model.Subject = "App支付测试DoNet";
            //model.TotalAmount = "0.01";
            //model.ProductCode = "QUICK_MSECURITY_PAY";
            //Random rad = new Random();
            //string NoId = rad.Next(1000, 9999).ToString() + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)] + rad.Next(1000, 9999) + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)];
            //model.OutTradeNo = NoId;
            //model.TimeoutExpress = "30m";
            //request.SetBizModel(model);
            ////这里和普通的接口调用不同，使用的是sdkExecute
            //AlipayTradeAppPayResponse response = client.SdkExecute(request);
            ////HttpUtility.HtmlEncode是为了输出到页面时防止被浏览器将关键参数html转义，实际打印到日志以及http传输不会有这个问题
            //Response.Write(HttpUtility.HtmlEncode(response.Body));
            ////页面输出的response.Body就是orderString 可以直接给客户端请求，无需再做处理。
            #endregion

            #region 电脑网站支付
            Random rad = new Random();
            string NoId = rad.Next(1000, 9999).ToString() + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)] + rad.Next(1000, 9999) + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)];
            IAopClient client = new DefaultAopClient(AlipayPaymentConfigHelper.Server_Url, AlipayPaymentConfigHelper.App_Id, AlipayPaymentConfigHelper.PrivateKeyPem, AlipayPaymentConfigHelper.Format, AlipayPaymentConfigHelper.Version, AlipayPaymentConfigHelper.Sign_Type, AlipayPaymentConfigHelper.PublicKeyPem, AlipayPaymentConfigHelper.Charset, false);
            AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
            request.BizContent = "{" +
            "    \"body\":\"Iphone6 16G\"," +
            "    \"subject\":\"Iphone6 16G\"," +
            "    \"out_trade_no\":\"" + NoId + "\"," +
            "    \"total_amount\":88.88," +
            "    \"product_code\":\"FAST_INSTANT_TRADE_PAY\"" +
            "  }";
            AlipayTradePagePayResponse response = client.pageExecute(request);
            //string form = response.Body;
            #endregion
            
            OperationResult result = new OperationResult(OperationResultType.Success, "", response.Body.ToString());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ViewResult RetrunMethod()
        {
            IDictionary<string, string> dic = GetRequestPost();

            if (dic == null)
            {
                dic.Add("OrderNo", "失败");
                AddCookies(dic);
                return View("Index");
            }

            dic.Add("OrderNo", dic["out_trade_no"]);
            AddCookies(dic);
            return View("Index");
        }

        private void AddCookies(IDictionary<string, string> dics)
        {
            foreach (var dic in dics)
            {
                if (Request.Cookies[dic.Key] != null)
                {
                    Request.Cookies[dic.Key].Value = dic.Value;
                    continue;
                }
                HttpCookie aCookie = new HttpCookie(dic.Key);
                aCookie.Value = dic.Value;
                aCookie.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(aCookie);
            }
        }

        //切记alipaypublickey是支付宝的公钥，请去open.alipay.com对应应用下查看。
        //bool RSACheckV1(IDictionary<string, string> parameters, string alipaypublicKey, string charset, string signType, bool keyFromFile)
        //bool flag = AlipaySignature.RSACheckV1(GetRequestPost(), TT.GetPublicKeyPem(), DefaultAopClient.CHARSET, "RSA2", false);


        public IDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
    }

    public class TT
    {
        /// <summary>
        /// 商户私钥
        /// </summary>
        /// <returns></returns>
        public static string GetPrivateKeyPem()
        {
            return GetCurrentPath() + "aop-sandbox-RSA-private-c#.pem";
        }

        /// <summary>
        /// 商户公钥
        /// </summary>
        /// <returns></returns>
        public static string GetPublicKeyPem()
        {
            return GetCurrentPath() + "public-key.pem";
        }

        private static string GetCurrentPath()
        {
            string basePath = System.IO.Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName;
            return basePath + "/Test/";
        }
    }
}