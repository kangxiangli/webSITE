using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace Whiskey.Web.Net.WeChatPay
{
    public static class WeChatPayCore
    {

         

        /// <summary>
        /// 拼接提交数据
        /// </summary>
        /// <param name="notityInfo">数据对象</param>
        /// <param name="formate">all表示拼接所用，nosign表示不拼接sign</param>
        /// <returns>拼接字符串</returns>
        //public static string GetData(WeChatPayNotifyInfo notityInfo, string formate, Encoding Code)
        //{
        //    SortedDictionary<string, string> sd = new SortedDictionary<string, string>();
        //    #region 添加参数
                        
        //    if (!string.IsNullOrEmpty(notityInfo.Appid))
        //    {
        //        sd.Add("appid", notityInfo.Appid);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.MchId))
        //    {
        //        sd.Add("mch_id", notityInfo.MchId);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.DeviceInfo))
        //    {
        //        sd.Add("device_info", notityInfo.DeviceInfo);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.NonceStr))
        //    {
        //        sd.Add("nonce_str", notityInfo.NonceStr);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.Body))
        //    {
        //        sd.Add("body", notityInfo.Body);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.Detail))
        //    {
        //        sd.Add("detail", notityInfo.Detail);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.Attach))
        //    {
        //        sd.Add("attach", notityInfo.Attach);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.OutTradeNo))
        //    {
        //        sd.Add("out_trade_no", notityInfo.OutTradeNo);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.FeeType))
        //    {
        //        sd.Add("fee_type", notityInfo.FeeType);
        //    }
        //    sd.Add("total_fee", notityInfo.TotalFee.ToString());
        //    if (!string.IsNullOrEmpty(notityInfo.SpbillCreateIp))
        //    {
        //        sd.Add("spbill_create_ip", notityInfo.SpbillCreateIp);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.TimeStart))
        //    {
        //        sd.Add("time_start", notityInfo.TimeStart);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.TimeExpire))
        //    {
        //        sd.Add("time_expire", notityInfo.TimeExpire);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.GoodsTag))
        //    {
        //        sd.Add("goods_tag", notityInfo.GoodsTag);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.NotifyUrl))
        //    {
        //        sd.Add("notify_url", notityInfo.NotifyUrl);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.TradeType))
        //    {
        //        sd.Add("trade_type", notityInfo.TradeType);
        //    }
        //    if (!string.IsNullOrEmpty(notityInfo.LimitPay))
        //    {
        //        sd.Add("limit_pay", notityInfo.LimitPay);
        //    }
        //    if (formate.Trim().ToLower()=="all")
        //    {
        //        if (!string.IsNullOrEmpty(notityInfo.Sign))
        //        {
        //            sd.Add("sign", notityInfo.Sign);
        //        }
        //    }
        //    #endregion

        //    StringBuilder sbData= new StringBuilder();
        //    foreach (var item in sd)
        //    {
        //        sbData.Append(item.Key + "=" + HttpUtility.UrlEncode(item.Value, Code) + "&");
        //    }
        //    int length = sbData.Length;
        //    sbData= sbData.Remove(length - 1, 1);
        //    return sbData.ToString();
        //}
         
    }
}
