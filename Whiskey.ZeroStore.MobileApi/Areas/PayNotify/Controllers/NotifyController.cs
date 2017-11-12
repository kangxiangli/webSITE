
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Models.WxPay;
using WxPayAPI;

namespace Whiskey.ZeroStore.MobileApi.Areas.PayNotify.Controllers
{
    public class NotifyController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(NotifyController));
        protected readonly IRechargeOrderContract _rechargeOrderContract;
        public NotifyController(IRechargeOrderContract rechargeOrderContract)
        {
            _rechargeOrderContract = rechargeOrderContract;
        }
        public void ResultNotifyHandler()
        {
            ResultNotify result = new ResultNotify(Request.InputStream);
            WxPayData handlerRes = result.ProcessNotify();
            string out_trade_no = handlerRes.GetValue("out_trade_no").ToString();
            //支付成功后处理
            if (handlerRes.GetValue("return_code").ToString() == "SUCCESS")
            {
                string transaction_id = handlerRes.GetValue("transaction_id").ToString();

                var status = _rechargeOrderContract.RechargeOrders.Where(x => x.Prepay_Id == transaction_id && x.order_Uid == out_trade_no)
                    .Select(x => x.pay_status).ToList().FirstOrDefault();
                if (status != 1)
                {
                    //status 0 订单生成 1 支付成功 2 支付失败
                    var res = _rechargeOrderContract.PaymentSuccess(transaction_id, 1, out_trade_no);
                }
            }
            else
            {
                //支付失败处理
                if (out_trade_no != "")
                {
                    var res = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                }
            }
        }

    }
}