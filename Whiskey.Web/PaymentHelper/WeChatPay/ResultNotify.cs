using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using WxPayAPI;

namespace Whiskey.ZeroStore.MobileApi.Models.WxPay
{
    public class ResultNotify
    {
        public Stream inputStream { get; set; }
        public ResultNotify(Stream stream)
        {
            this.inputStream = stream;
        }
        /// <summary>
        /// 接收从微信支付后台发送过来的数据并验证签名
        /// </summary>
        /// <returns>微信支付后台返回的数据</returns>
        public WxPayData GetNotifyData()
        {
            //接收从微信后台POST过来的数据
            System.IO.Stream s = inputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();
            Log.Info(this.GetType().ToString(), "Receive data from WeChat : " + builder.ToString());
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误处理
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
            }
            return data;
        }

        public WxPayData ProcessNotify()
        {
            WxPayData notifyData = GetNotifyData();
            Log.Debug(this.GetType().ToString(), "微信返回数据 : " + notifyData.ToXml());
            WxPayData res = new WxPayData();
            //string out_trade_no = notifyData.IsSet("out_trade_no").ToString();
            string out_trade_no = notifyData.GetValue("out_trade_no").ToString();
            res.SetValue("out_trade_no", out_trade_no);
            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id, 2))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                Log.Error(this.GetType().ToString(), "Order query failure : " + res.ToXml());
            }
            //查询订单成功
            else
            {
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                res.SetValue("transaction_id", transaction_id);
            }
            Log.Debug(this.GetType().ToString(), "微信返回数据  整理 : " + res.ToXml());
            return res;
        }

        //查询订单
        private bool QueryOrder(string transaction_id, int appId = 1)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req, 6, appId);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}