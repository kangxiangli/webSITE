using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.PaymentHelper;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Models.WxPay;
using WxPayAPI;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance.Controllers
{
    public class ResultNotifyController : Controller
    {
        
        protected readonly IRechargeOrderContract _rechargeOrderContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberdepositContract;
        public ResultNotifyController(IRechargeOrderContract rechargeOrderContract, IMemberContract memberContract,
            IMemberDepositContract memberdepositContract)
        {
            _rechargeOrderContract = rechargeOrderContract;
            _memberContract = memberContract;
            _memberdepositContract = memberdepositContract;
        }
        public ActionResult Notify()
        {
            Log.Error(this.GetType().ToString(), "transaction_id===>微信回调开始");
            ResultNotify result = new ResultNotify(Request.InputStream);
            WxPayData handlerRes = result.ProcessNotify();
            string out_trade_no = handlerRes.GetValue("out_trade_no").ToString();
            string resXml = string.Empty;
            Log.Error(this.GetType().ToString(), "transaction_id===>111111");
            //支付成功后处理
            if (handlerRes.GetValue("return_code").ToString() == "SUCCESS")
            {
                resXml = "<xml>" + "<return_code><![CDATA[SUCCESS]]></return_code>"
+ "<return_msg><![CDATA[OK]]></return_msg>" + "</xml> ";
                string transaction_id = handlerRes.GetValue("transaction_id").ToString();
                Log.Error(this.GetType().ToString(), "transaction_id===>"+ transaction_id);
                WxPayData orderCheck = null; //CheckTransactionidPayState(transaction_id, out_trade_no);
                if (true)
                {
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
                                        Price = x.RechargeType == 0 ? x.TureAmount : 0,
                                        Score = x.RechargeType == 1 ? x.TureAmount : 0,
                                        Cash = x.Amount,
                                        order_Uid = x.order_Uid,
                                        MemberDepositType = 2,
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
                }
                else
                {
                    resXml = "<xml>" + "<return_code><![CDATA[FAIL]]></return_code>"
    + "<return_msg><![CDATA[支付失败]]></return_msg>" + "</xml> ";
                }
            }
            else
            {
                resXml = "<xml>" + "<return_code><![CDATA[FAIL]]></return_code>"
+ "<return_msg><![CDATA[支付失败]]></return_msg>" + "</xml> ";
                //支付失败处理
                if (out_trade_no != "")
                {
                    var res = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                }
            }
            Response.Write(resXml);
            Response.End();
            return View();
        }

        //查询微信支付订单状态
        public JsonResult CheckTransactionidPayState(string out_trade_no)
        {
            WxPayData data = new WxPayData();
            data.SetValue("appid", PaymentConfigHelper.WxAppId);
            data.SetValue("mch_id", PaymentConfigHelper.WxMCHID);//商户号
            data.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));
            data.SetValue("transaction_id", "");
            data.SetValue("out_trade_no", out_trade_no);
            WxPayData result=WxPayApi.OrderQuery(data);

            return Json(new OperationResult(OperationResultType.Success, "获取成功！", result), JsonRequestBehavior.AllowGet);
        }

    }
}