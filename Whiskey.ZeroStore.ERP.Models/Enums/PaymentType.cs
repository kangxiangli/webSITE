using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 支付方式枚举，默认线下支付
    /// 线下支付：0
    /// 支付宝：coming soon
    /// 微信：comming soon
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// 线下支付
        /// </summary>
        Offline = 0
    }
    /// <summary>
    /// 采购单支付方式
    /// </summary>
    public enum PaymentPurchaseType
    {
        吊牌价=0,
        采购价,
        进货价
    }
}
