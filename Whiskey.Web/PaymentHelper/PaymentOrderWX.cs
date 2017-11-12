using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.PaymentHelper
{
  public  class PaymentOrderWX
    {
        //充值金额
        public int Amount { get; set; }
        //充值类型 0 储值 1 积分
        public int RechargeType { get; set; }
        //充值规则ID
        public int RuleTypeId { get; set; }
        //兑换后的储值金额（积分）
        public int TureAmount { get; set; }
        /// <summary>
        /// 支付方式 1 微信 2 支付宝
        /// </summary>
        public int Pay_Type { get; set; }
        //商品描述
        public string body { get; set; }
        /// <summary>
        /// 微信支付类型  移动端 APP 扫码 NATIVE
        /// </summary>
        public string trade_type { get; set; }
        /// <summary>
        /// app类型 1零时尚 2 小蝶办公 
        /// </summary>
        public int appType { get; set; }
        //支付金额 单位 分
        public int total_fee { get; set; }
        //时间戳
        public string timestamp { get; set; }
        //签名标记
        public string sign { get; set; }
        //充值描述 例如（零时尚店铺充值）
        public string attach { get; set; }
    }
}
