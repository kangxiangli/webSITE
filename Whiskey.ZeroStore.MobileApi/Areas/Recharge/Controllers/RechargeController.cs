
using Aop.Api.Domain;
using Aop.Api.PaymentHelper;
using Aop.Api.Util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.PaymentHelper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.MobileApi.Models.WxPay;
using WxPayAPI;
using WxPayTest.lib;

namespace Whiskey.ZeroStore.MobileApi.Areas.Recharge.Controllers
{
    //[License(CheckMode.Verify)]
    public class RechargeController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(RechargeController));
        protected readonly IRechargeOrderContract _rechargeOrderContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberActivityContract _memberActivityContract;
        protected readonly IMemberDepositContract _memberdepositContract;
        protected readonly IStoreValueRuleContract _storeValueRuleContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IOrderContract _orderContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IRetailContract _retailContract;
        public RechargeController(IRechargeOrderContract rechargeOrderContract, IMemberContract memberContract,
            IMemberDepositContract memberdepositContract,
            IMemberActivityContract memberActivityContract,
            IStoreValueRuleContract storeValueRuleContract,
            IAdministratorContract administratorContract,
            IOrderContract orderContract,
            IMemberDepositContract memberDepositContract,
            IRetailContract retailContract)
        {
            _rechargeOrderContract = rechargeOrderContract;
            _memberContract = memberContract;
            _memberdepositContract = memberdepositContract;
            _memberActivityContract = memberActivityContract;
            _storeValueRuleContract = storeValueRuleContract;
            _administratorContract = administratorContract;
            _orderContract = orderContract;
            _memberDepositContract = memberDepositContract;
            _retailContract = retailContract;
        }
        #endregion

        public JsonResult Index()
        {
            return Json(new WxPayInfo());
        }

        #region 生成预支付订单
        #region 微信生成预支付订单
        /// <summary>
        /// 微信生成预支付订单
        /// </summary>
        /// <returns></returns>
        public JsonResult UnifiedOrder()
        {
            Dictionary<string, object> m_value = new Dictionary<string, object>();
            string jsonstr = Request["paymentorder"];
            int memberId = Request["memberId"] == null ? 0 : Convert.ToInt32(Request["memberId"]);
            int adminId = Request["adminId"] == null ? 0 : Convert.ToInt32(Request["adminId"]);
            int urlAdr = 2;
            if (adminId > 0)
            {
                urlAdr = 2;
                memberId = _administratorContract.GetMemberId(adminId);
            }
            PaymentOrderWX paymentorder = new PaymentOrderWX();
            if (!string.IsNullOrEmpty(jsonstr))
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                paymentorder = js.Deserialize<PaymentOrderWX>(jsonstr);
            }
            if (paymentorder.Pay_Type != 1)
            {
                return Json(new OperationResult(OperationResultType.Error, "支付方式错误"));
            }
            int Id = paymentorder.RuleTypeId;
            decimal Amount = paymentorder.Amount;

            var Price = _storeValueRuleContract.StoreValueRules.Where(x => x.Id == Id).ToList().FirstOrDefault();
            var errordata = new
            {
                prepayid = "",
                out_trade_no = "",
                sign = ""
            };
            if (Price == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-当前充值规则不匹配！", errordata), JsonRequestBehavior.AllowGet);
            }
            if (Amount != Price.Price)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-充值金额不匹配！", errordata), JsonRequestBehavior.AllowGet);
            }
            
            paymentorder.total_fee = 1;         //实际支付金额 测试完毕后要注释
            FashionData data = new FashionData();
            data.SetValue("Amount", paymentorder.Amount);//充值金额
            data.SetValue("RechargeType", paymentorder.RechargeType);//充值类型
            data.SetValue("RuleTypeId", paymentorder.RuleTypeId);//充值规则
            data.SetValue("TureAmount", paymentorder.TureAmount);//兑换后的金额（积分）
                                                                 //data.SetValue("Pay_Type", paymentorder.Pay_Type);//1 微信 2 支付宝
            data.SetValue("body", paymentorder.body);//商品信息描述
            data.SetValue("trade_type", "APP");
            data.SetValue("appType", paymentorder.appType);//1 零时尚 app 2 小蝶办公
                                                           //data.SetValue("total_fee", paymentorder.total_fee);
                                                           //data.SetValue("timestamp", paymentorder.timestamp);
            data.SetValue("sign", paymentorder.sign);

            string prepay_id = string.Empty;
            //验证签名是否正确 和请求是否过期
            if (!data.CheckTimspan(paymentorder.timestamp)  /*&&data.CheckSign()*/)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-验证签名失败！", errordata), JsonRequestBehavior.AllowGet);
            }
            RechargeOrder order = new RechargeOrder();
            order.Amount = paymentorder.Amount;
            order.RechargeType = (MemberActivityFlag)paymentorder.RechargeType;
            order.RuleTypeId = paymentorder.RuleTypeId;
            order.TureAmount = paymentorder.TureAmount;
            order.Pay_Type = paymentorder.Pay_Type;
            //订单号生成规则 当前时间+随机5位数
            order.UserId = memberId;
            string out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmssffff").ToString() + new Random().Next(10000, 99999).ToString();
            order.order_Uid = out_trade_no;
            var res = _rechargeOrderContract.Insert(order);
            if (res.ResultType != OperationResultType.Success)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-系统生成支付订单失败！", errordata), JsonRequestBehavior.AllowGet);
            }
            var product_id = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no).Select(x => x.Id).FirstOrDefault();
            WxPayData wxdata = new WxPayData();
            wxdata.SetValue("body", paymentorder.body);//商品描述 
            wxdata.SetValue("attach", paymentorder.attach);//充值描述
            wxdata.SetValue("out_trade_no", out_trade_no);
            //总金额    订单总金额，只能为整数，详见支付金额   注意：金额不带小数点，最小为分，即1元=100分，传数据即可
            wxdata.SetValue("total_fee", paymentorder.total_fee);
            wxdata.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            wxdata.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            wxdata.SetValue("trade_type", "APP");
            wxdata.SetValue("product_id", product_id);
            wxdata.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));

            string appId = "";
            string wxMchID = "";
            switch (paymentorder.appType)
            {
                case 1:
                    appId = PaymentConfigHelper.Wx_0Fashion_AppId;
                    wxMchID = PaymentConfigHelper.Wx_0Fashion_MCHID;
                    break;
                case 2:
                    appId = PaymentConfigHelper.Wx_XD_AppId;
                    wxMchID = PaymentConfigHelper.Wx_XD_MCHID;
                    break;
                default:
                    break;
            }
            wxdata.SetValue("appid", appId);//公众账号ID
            wxdata.SetValue("mch_id", wxMchID);//商户号
            wxdata.SetValue("spbill_create_ip", PaymentConfigHelper.Ip);//终端ip
            WxPayData result = WxPayApi.UnifiedOrder(wxdata, 6, paymentorder.appType, urlAdr);
            if (result == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "测试过去了"));
            }
            if (result.GetValue("return_code").ToString() != "SUCCESS" || result.GetValue("result_code").ToString() != "SUCCESS")
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-生成预支付订单失败！", errordata), JsonRequestBehavior.AllowGet);
            }
            prepay_id = result.GetValue("prepay_id").ToString();
            WxPayData make_sign = new WxPayData();
            make_sign.SetValue("appid", appId);
            make_sign.SetValue("partnerid", wxMchID);//商户号
            string nonce_str = Guid.NewGuid().ToString().Replace("-", "");
            make_sign.SetValue("noncestr", nonce_str);//随机字符串
            make_sign.SetValue("prepayid", prepay_id);
            make_sign.SetValue("package", "Sign=WXPay");
            int ts2 = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            make_sign.SetValue("timestamp", ts2);
            string ios_sign = make_sign.MakeSign();
            var successdata = new
            {
                prepayid = prepay_id,
                out_trade_no = out_trade_no,
                sign = ios_sign,
                nonce_str = nonce_str,
                timestamp = ts2
            };
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", successdata), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 支付宝生成预支付订单
        /// <summary>
        /// 支付宝生成预支付订单
        /// </summary>
        /// <returns></returns>
        public JsonResult ZFBUnifiedOrder()
        {
            Dictionary<string, object> m_value = new Dictionary<string, object>();
            string jsonstr = Request["paymentorder"];
            int memberId = Request["memberId"] == null ? 0 : Convert.ToInt32(Request["memberId"]);
            int adminId = Request["adminId"] == null ? 0 : Convert.ToInt32(Request["adminId"]);
            if (adminId > 0)
            {
                memberId = _administratorContract.GetMemberId(adminId);
            }
            PaymentOrderWX paymentorder = new PaymentOrderWX();
            if (!string.IsNullOrEmpty(jsonstr))
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                paymentorder = js.Deserialize<PaymentOrderWX>(jsonstr);
            }
            if (paymentorder.Pay_Type != 2)
            {
                return Json(new OperationResult(OperationResultType.Error, "支付方式错误"));
            }
            int Id = paymentorder.RuleTypeId;
            decimal Amount = paymentorder.Amount;

            var Price = _storeValueRuleContract.StoreValueRules.Where(x => x.Id == Id).ToList().FirstOrDefault();
            var errordata = new
            {
                prepayid = "",
                out_trade_no = "",
                sign = ""
            };
            if (Price == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-当前充值规则不匹配！", errordata), JsonRequestBehavior.AllowGet);
            }
            if (Amount != Price.Price)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-充值金额不匹配！", errordata), JsonRequestBehavior.AllowGet);
            }

            RechargeOrder order = new RechargeOrder();
            order.Amount = paymentorder.Amount;
            order.RechargeType = (MemberActivityFlag)paymentorder.RechargeType;
            order.RuleTypeId = paymentorder.RuleTypeId;
            order.TureAmount = paymentorder.TureAmount;
            order.Pay_Type = paymentorder.Pay_Type;
            //订单号生成规则 当前时间+随机5位数
            order.UserId = memberId;
            string out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmssffff").ToString() + new Random().Next(10000, 99999).ToString();
            order.order_Uid = out_trade_no;
            var res = _rechargeOrderContract.Insert(order);
            if (res.ResultType != OperationResultType.Success)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败-系统生成支付订单失败！", errordata), JsonRequestBehavior.AllowGet);
            }
            var product_id = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no).Select(x => x.Id).FirstOrDefault();

            AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
            model.Body = "积分充值";
            model.Subject = "积分充值";
            model.TotalAmount = (paymentorder.total_fee / 100).ToString("f2");
            model.TotalAmount = "0.01";               //实际支付金额 测试完毕后要注释
            model.ProductCode = "QUICK_MSECURITY_PAY";
            //Random rad = new Random();
            //string NoId = rad.Next(1000, 9999).ToString() + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)] + rad.Next(1000, 9999) + Guid.NewGuid().ToString().Split('-')[rad.Next(1, 4)];
            model.OutTradeNo = out_trade_no;
            model.TimeoutExpress = "30m";
            string result = AopPaymentHelper.UnifiedOrder(model);

            if (result == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "测试过去了"));
            }

            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("content", result);
            OperationResult opera = new OperationResult(OperationResultType.Success, "", dic);
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region 获取真实ip
        /// <summary>  
        /// 获取真实ip  
        /// </summary>  
        /// <returns></returns>  
        public string GetRealIP()
        {
            string result = String.Empty;
            result = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //可能有代理   
            if (!string.IsNullOrWhiteSpace(result))
            {
                //没有"." 肯定是非IP格式  
                if (result.IndexOf(".") == -1)
                {
                    result = null;
                }
                else
                {
                    //有","，估计多个代理。取第一个不是内网的IP。  
                    if (result.IndexOf(",") != -1)
                    {
                        result = result.Replace(" ", string.Empty).Replace("\"", string.Empty);

                        string[] temparyip = result.Split(",;".ToCharArray());

                        if (temparyip != null && temparyip.Length > 0)
                        {
                            for (int i = 0; i < temparyip.Length; i++)
                            {
                                //找到不是内网的地址  
                                if (IsIPAddress(temparyip[i]) && temparyip[i].Substring(0, 3) != "10." && temparyip[i].Substring(0, 7) != "192.168" && temparyip[i].Substring(0, 7) != "172.16.")
                                {
                                    return temparyip[i];
                                }
                            }
                        }
                    }
                    //代理即是IP格式  
                    else if (IsIPAddress(result))
                    {
                        return result;
                    }
                    //代理中的内容非IP  
                    else
                    {
                        result = null;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = System.Web.HttpContext.Current.Request.UserHostAddress;
            }
            return result;
        }
        public bool IsIPAddress(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length < 7 || str.Length > 15)
                return false;

            string regformat = @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(str);
        }
        #endregion

        #region 支付结果回调通知
        #region 微信支付结果回调通知
        /// <summary>
        /// 微信支付结果回调通知
        /// </summary>
        public void ResultNotifyHandler()
        {
            try
            {
                ResultNotify result = new ResultNotify(Request.InputStream);
                WxPayData handlerRes = result.ProcessNotify();
                string out_trade_no = handlerRes.GetValue("out_trade_no").ToString();

                WxPayAPI.Log.Debug(this.GetType().ToString(), "订单号及是否成功 : " + out_trade_no + "        " + handlerRes.GetValue("return_code").ToString());
                if (handlerRes == null || handlerRes.GetValue("return_code") == null || handlerRes.GetValue("return_code").ToString() != "SUCCESS")
                {
                    //支付失败处理
                    if (out_trade_no != "")
                    {
                        var res = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                    }
                    return;
                }


                string prepay_id = handlerRes.GetValue("transaction_id").ToString();
                WxPayAPI.Log.Debug(this.GetType().ToString(), "微信 transaction_id : " + prepay_id);

                var model = _rechargeOrderContract.RechargeOrders.FirstOrDefault(x => x.order_Uid == out_trade_no);

                model.Prepay_Id = prepay_id;
                _rechargeOrderContract.Update(model);

                //支付成功后处理
                var status = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                    .Select(x => x.pay_status).ToList().FirstOrDefault();

                WxPayAPI.Log.Debug(this.GetType().ToString(), "微信 status : " + status);
                if (status == 1)
                {
                    return;
                }
                using (var a = _rechargeOrderContract.GetTransaction())
                {

                    //status 0 订单生成 1 支付成功 2 支付失败
                    var res = _rechargeOrderContract.PaymentSuccess(prepay_id, 1, out_trade_no);
                    if (res.ResultType == OperationResultType.Error)
                    {
                        a.Rollback();
                        return;
                    }

                    var b = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                        .Select(x => new MemberDepositDto()
                        {
                            MemberId = x.UserId,
                            Price = (decimal)(x.RechargeType == MemberActivityFlag.Recharge ? x.TureAmount : 0),
                            Score = (decimal)(x.RechargeType == MemberActivityFlag.Score ? x.TureAmount : 0),
                            Cash = (decimal)x.Amount,
                            order_Uid = x.order_Uid,
                            MemberDepositType = MemberDepositFlag.System,
                            MemberActivityType = x.RechargeType
                        }).FirstOrDefault();

                    var updateMember = _memberdepositContract.InsertWx(b);
                    if (updateMember.ResultType == OperationResultType.Error)
                    {
                        a.Rollback();
                    }
                    else
                    {
                        //更新成功  
                        a.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                WxPayAPI.Log.Error("WxPayApi_notify_url", "Error：" + ex.ToString());
            }
        }

        /// <summary>
        /// 微信支付结果回调通知
        /// </summary>
        public void ResultNotifyWXHandler()
        {
            try
            {
                ResultNotify result = new ResultNotify(Request.InputStream);
                WxPayData handlerRes = result.ProcessNotify();
                string out_trade_no = handlerRes.GetValue("out_trade_no").ToString();

                WxPayAPI.Log.Debug(this.GetType().ToString(), "订单号及是否成功 : " + out_trade_no + "        " + handlerRes.GetValue("return_code").ToString());

                if (handlerRes == null || handlerRes.GetValue("return_code") == null || handlerRes.GetValue("return_code").ToString() != "SUCCESS")
                {
                    //支付失败处理
                    if (out_trade_no != "")
                    {
                        var res = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                    }
                    return;
                }

                string prepay_id = handlerRes.GetValue("transaction_id").ToString();
                WxPayAPI.Log.Debug(this.GetType().ToString(), "微信 transaction_id : " + prepay_id);

                var model = _rechargeOrderContract.RechargeOrders.FirstOrDefault(x => x.order_Uid == out_trade_no);

                model.Prepay_Id = prepay_id;
                _rechargeOrderContract.Update(model);

                //支付成功后处理
                var status = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                    .Select(x => x.pay_status).ToList().FirstOrDefault();

                WxPayAPI.Log.Debug(this.GetType().ToString(), "微信 status : " + status);
                if (status == 1)
                {
                    return;
                }

                using (var a = _rechargeOrderContract.GetTransaction())
                {
                    //status 0 订单生成 1 支付成功 2 支付失败
                    var res = _rechargeOrderContract.PaymentSuccess(prepay_id, 1, out_trade_no);
                    if (res.ResultType == OperationResultType.Error)
                    {
                        a.Rollback();
                        return;
                    }

                    var b = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                        .Select(x => new MemberDepositDto()
                        {
                            MemberId = x.UserId,
                            Price = (decimal)(x.RechargeType == MemberActivityFlag.Recharge ? x.TureAmount : 0),
                            Score = (decimal)(x.RechargeType == MemberActivityFlag.Score ? x.TureAmount : 0),
                            Cash = (decimal)x.Amount,
                            order_Uid = x.order_Uid,
                            MemberDepositType = MemberDepositFlag.System,
                            MemberActivityType = x.RechargeType
                        }).FirstOrDefault();

                    var updateMember = _memberdepositContract.InsertWxAdminId(b);
                    if (updateMember.ResultType == OperationResultType.Error)
                    {
                        a.Rollback();
                    }
                    else
                    {
                        //更新成功  
                        a.Commit();
                    }
                    WxPayAPI.Log.Debug("WxPayApi_notify_url", "进来了");
                }
            }
            catch (Exception ex)
            {
                WxPayAPI.Log.Error("WxPayApi_notify_url", "Error：" + ex.ToString());
            }
        }
        #endregion

        #region 支付宝支付结果回调通知
        /// <summary>
        /// 支付宝支付结果回调通知
        /// </summary>
        /// <returns></returns>
        public JsonResult ResultNotifyZFBHandler()
        {
            try
            {
                IDictionary<string, string> dic = AopPaymentHelper.GetRequestPost(Request);

                string sign_type = dic["sign_type"];
                bool signVerified = AlipaySignature.RSACheckV2(dic, AlipayPaymentConfigHelper.PublicKeyPemAlipay, AlipayPaymentConfigHelper.Charset, sign_type, false); //调用SDK验证签名

                if (!signVerified)
                {// TODO 验签失败则记录异常日志，并在response中返回failure.
                    return Json("error", JsonRequestBehavior.AllowGet);
                }

                // TODO 验签成功后
                //按照支付结果异步通知中的描述，对支付结果中的业务内容进行1\2\3\4二次校验，校验成功后在response中返回success，校验失败返回failure


                string out_trade_no = dic["out_trade_no"].ToString();
                if (dic["trade_status"].ToString() != "TRADE_SUCCESS")
                {
                    //支付失败处理
                    if (out_trade_no != "")
                    {
                        var res = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                    }
                }

                var model = _rechargeOrderContract.RechargeOrders.FirstOrDefault(x => x.order_Uid == out_trade_no);

                string transaction_id = dic["trade_no"].ToString();
                model.Prepay_Id = transaction_id;
                _rechargeOrderContract.Update(model);


                //支付成功后处理
                var status = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                    .Select(x => x.pay_status).ToList().FirstOrDefault();

                if (status == 1)
                {
                    return Json("success", JsonRequestBehavior.AllowGet);
                }

                using (var a = _rechargeOrderContract.GetTransaction())
                {

                    //status 0 订单生成 1 支付成功 2 支付失败
                    var res = _rechargeOrderContract.PaymentSuccess(transaction_id, 1, out_trade_no);
                    if (res.ResultType == OperationResultType.Error)
                    {
                        a.Rollback();
                    }
                    else
                    {

                        var b = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                            .Select(x => new MemberDepositDto()
                            {
                                MemberId = x.UserId,
                                Price = (decimal)(x.RechargeType == MemberActivityFlag.Recharge ? x.TureAmount : 0),
                                Score = (decimal)(x.RechargeType == MemberActivityFlag.Score ? x.TureAmount : 0),
                                Cash = (decimal)x.Amount,
                                order_Uid = x.order_Uid,
                                MemberDepositType = MemberDepositFlag.System,
                                MemberActivityType = x.RechargeType
                            }).FirstOrDefault();

                        var updateMember = _memberdepositContract.InsertWxAdminId(b);
                        if (updateMember.ResultType == OperationResultType.Error)
                        {
                            a.Rollback();
                        }
                        else
                        {
                            //更新成功
                            a.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WxPayAPI.Log.Error("WxPayApi_notify_url", "Error：" + ex.ToString());
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region 注释代码
        //public String Get_Http(String a_strUrl, int timeout)
        //{
        //    string strResult;
        //    try
        //    {
        //        HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(a_strUrl);
        //        myReq.Timeout = timeout;
        //        HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
        //        Stream myStream = HttpWResp.GetResponseStream();
        //        StreamReader sr = new StreamReader(myStream, Encoding.Default);
        //        StringBuilder strBuilder = new StringBuilder();
        //        while (-1 != sr.Peek())
        //        {
        //            strBuilder.Append(sr.ReadLine());
        //        }

        //        strResult = strBuilder.ToString();
        //    }
        //    catch (Exception exp)
        //    {

        //        strResult = "错误：" + exp.Message;
        //    }

        //    return strResult;
        //}
        #endregion

        #region 获取平台订单信息（微信）
        //获取平台订单信息
        public JsonResult GetOrderInfo()
        {
            FashionData data = new FashionData();
            string timestamp = Request["timestamp"];
            data.SetValue("timestamp", timestamp);
            string sign = Request["sign"];
            data.SetValue("sign", sign);
            string order_Uid = Request["order_Uid"];
            data.SetValue("order_Uid", order_Uid);
            List<RechargeOrder> list = new List<RechargeOrder>();
            if (data.CheckTimspan(timestamp) && data.CheckSign())
            {
                list = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == order_Uid).ToList();
            }
            return Json(list);
        }
        #endregion

        #region 获取充值规则
        /// <summary>
        /// 获取充值规则
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="RuleType"></param>
        /// <returns></returns>
        public JsonResult GetStoreValueRules(int Id, int RuleType)
        {
            var MemberId = Id;// _administratorContract.Administrators.FirstOrDefault(x => x.Id == Id).MemberId;
            var memberType = _memberContract.Members.Where(x => x.Id == MemberId).Select(x => x.MemberTypeId).FirstOrDefault();
            var scoreList = new List<object>();
            var Score = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == true && x.RuleType == RuleType)
                 .Select(c => new
                 {
                     c.Id,
                     c.StoreValueName,
                     Name = c.MemberTypes.Select(x => x.Id),
                     c.Price,
                     c.RewardMoney,
                     c.Score,
                     c.Operator.Member.MemberName,
                     c.ImageUrl
                 }).ToList();
            foreach (var item in Score)
            {
                //if (item.Name.Contains(memberType))
                //{
                scoreList.Add(item);
                //}
            }

            var ScoreForever = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == false && x.RuleType == RuleType)
                .Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                 .Select(c => new
                 {
                     c.Id,
                     c.StoreValueName,
                     Name = c.MemberTypes.Select(x => x.Id),
                     c.Price,
                     c.RewardMoney,
                     c.Score,
                     c.Operator.Member.MemberName,
                     c.ImageUrl
                 }).ToList();
            foreach (var item in ScoreForever)
            {
                //if (item.Name.Contains(memberType))
                //{
                scoreList.Add(item);
                //}
            }
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", scoreList), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 会员的积分变动记录
        /// <summary>
        /// 会员的积分变动记录
        /// </summary>
        /// <returns></returns>
        public JsonResult ScoreOrderList(int Id)
        {
            List<MemberDepositInfo> li = new List<MemberDepositInfo>();
            var Member_Id = Id;// _administratorContract.Administrators.FirstOrDefault(x => x.Id == Id).MemberId;
            var totalScore = _memberContract.Members.FirstOrDefault(x => x.Id == Member_Id).Score;
            //充值记录
            var depli =
                _memberDepositContract.MemberDeposits
                    .Where(c => c.MemberActivityType == MemberActivityFlag.Score &&
                    c.MemberId == Member_Id)
                    .OrderByDescending(c => c.CreatedTime)
                    .Select(c => new MemberDepositInfo()
                    {
                        MemberNumb = c.Member.UniquelyIdentifies,
                        CreateTime = c.CreatedTime,
                        Score = (float)c.Score,
                        Operator = c.Operator.Member.MemberName,
                        MemberId = c.MemberId,
                        MemberName = c.Member.MemberName,
                        Other = "充值"
                    }).ToList();
            li.AddRange(depli);

            int memberid = -1;
            if (li.Any())
            {
                memberid = depli.FirstOrDefault().MemberId;
            }
            if (memberid != -1)
            {
                var dali =
                    _retailContract.Retails
                        .Where(c => c.ConsumerId == memberid && c.ScoreConsume > 0)
                        .OrderByDescending(c => c.CreatedTime)
                        .Select(c => new MemberDepositInfo()
                        {
                            MemberNumb = c.Consumer.UniquelyIdentifies,
                            CreateTime = c.CreatedTime,
                            Score = (float)c.ScoreConsume,
                            Operator = c.Operator.Member.MemberName,
                            Other = "消费"
                        }).ToList();
                li.AddRange(dali);
                //消费获得赠送积分
                var daliGive =
                        _retailContract.Retails.Where(c => c.ConsumerId == memberid && c.ScoreConsume > 0)
                        .OrderByDescending(c => c.CreatedTime)
                        .Select(c => new MemberDepositInfo()
                        {
                            MemberNumb = c.Consumer.UniquelyIdentifies,
                            CreateTime = c.CreatedTime,
                            Score = (float)c.GetScore,
                            Operator = c.Operator.Member.MemberName,
                            Other = "消费赠送"
                        }).ToList();
                li.AddRange(daliGive);
            }
            var resul = li.OrderByDescending(c => c.CreateTime).Take(5).Select(c => new
            {
                c.MemberNumb,
                CreateTime = c.CreateTime.ToShortDateString(),
                c.Score,
                c.Operator,
                Type = c.Other
            }).ToList();
            var data = new
            {
                totalScore = totalScore,
                dataInfo = resul
            };
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", data), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 验证Ios 支付回调处理(微信)
        /// <summary>
        /// 验证Ios 支付回调处理（微信）
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckIosPay(string out_trade_no, string timestamp, string sign, int appId = 1)
        {
            FashionData fddata = new FashionData();
            fddata.SetValue("out_trade_no", out_trade_no);
            fddata.SetValue("timestamp", timestamp);
            fddata.SetValue("sign", sign);

            string wxAppId = "";
            string wxMchID = "";
            switch (appId)
            {
                case 1:
                    wxAppId = PaymentConfigHelper.Wx_0Fashion_AppId;
                    wxMchID = PaymentConfigHelper.Wx_0Fashion_MCHID;
                    break;
                case 2:
                    wxAppId = PaymentConfigHelper.Wx_XD_AppId;
                    wxMchID = PaymentConfigHelper.Wx_XD_MCHID;
                    break;
                default:
                    break;
            }
            if (fddata.CheckTimspan(timestamp) && fddata.CheckSign())
            {
                WxPayData data = new WxPayData();
                data.SetValue("appid", wxAppId);
                data.SetValue("mch_id", wxMchID);//商户号
                data.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));
                data.SetValue("transaction_id", "");
                data.SetValue("out_trade_no", out_trade_no);
                WxPayData result = WxPayApi.OrderQuery(data, 6, appId);
                string return_code = result.GetValue("return_code").ToString();
                string result_code = result.GetValue("result_code").ToString();
                string trade_state = result.GetValue("trade_state").ToString();//订单交易状态
                string msg = string.Empty;
                if (return_code == "SUCCESS" && result_code == "SUCCESS")
                {
                    switch (trade_state)
                    {

                        case "SUCCESS":
                            #region 支付成功处理
                            msg = "支付成功";
                            string transaction_id = result.GetValue("transaction_id").ToString();
                            var status = _rechargeOrderContract.RechargeOrders.Where(x => x.Prepay_Id == transaction_id && x.order_Uid == out_trade_no)
                            .Select(x => x.pay_status).ToList().FirstOrDefault();
                            if (status != 1)
                            {
                                using (var a = _rechargeOrderContract.GetTransaction())
                                {

                                    //status 0 订单生成 1 支付成功 2 支付失败
                                    var res = _rechargeOrderContract.PaymentSuccess(transaction_id, 1, out_trade_no);
                                    if (res.ResultType == OperationResultType.Error)
                                    {
                                        a.Rollback();
                                    }
                                    else
                                    {

                                        var b = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                                            .Select(x => new MemberDepositDto()
                                            {
                                                MemberId = x.UserId,
                                                Price = x.RechargeType == MemberActivityFlag.Recharge ? x.TureAmount : 0,
                                                Score = x.RechargeType == MemberActivityFlag.Score ? x.TureAmount : 0,
                                                Card = x.Amount,
                                                order_Uid = x.order_Uid,
                                                MemberDepositType = MemberDepositFlag.System,
                                                MemberActivityType = x.RechargeType
                                            }).FirstOrDefault();
                                        var updateMember = _memberdepositContract.InsertWx(b);
                                        if (updateMember.ResultType == OperationResultType.Error)
                                        {
                                            a.Rollback();
                                        }
                                        else
                                        {
                                            //更新成功  
                                            a.Commit();
                                        }
                                    }
                                }
                            }
                            #endregion
                            break;
                        case "PAYERROR":
                            #region 支付失败处理
                            msg = "支付失败";
                            var resFaile = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                            #endregion
                            break;
                        case "REFUND":
                            #region 转入退款
                            msg = "转入退款";
                            #endregion
                            break;
                        case "NOTPAY":
                            #region 未支付
                            msg = "未支付";
                            #endregion
                            break;
                        case "CLOSED":
                            #region 已关闭
                            msg = "已关闭";
                            #endregion
                            break;
                        case "REVOKED":
                            #region 已撤销（刷卡支付）
                            msg = "已撤销（刷卡支付）";
                            #endregion
                            break;
                        case "USERPAYING":
                            #region 用户支付中
                            msg = "USERPAYING";
                            #endregion
                            break;
                    }
                }
                var order = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no).FirstOrDefault();
                var orderState = (from c in _rechargeOrderContract.RechargeOrders
                                  where c.order_Uid == out_trade_no
                                  select c.pay_status).FirstOrDefault();
                var rusltdata = new
                {
                    trade_state = trade_state,
                    msg = msg
                };
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", rusltdata), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rusltdata = new
                {
                    trade_state = "",
                    msg = "签名验证失败"
                };
                return Json(new OperationResult(OperationResultType.Error, "获取失败！", rusltdata), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }

    public class MemberDepositInfo
    {
        public int MemberId { get; set; }
        public string MemberNumb { get; set; }
        public string MemberName { get; set; }
        public float Score { get; set; }
        public DateTime CreateTime { get; set; }
        public string Operator { get; set; }
        public string Other { get; set; }
    }
}
