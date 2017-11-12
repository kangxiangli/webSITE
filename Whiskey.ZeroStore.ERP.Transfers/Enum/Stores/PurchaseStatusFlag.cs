using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Stores
{
    public enum PurchaseStatusFlag
    {
        /// <summary>
        /// 采购中
        /// </summary>
        Purchasing,

        /// <summary>
        /// 配货完成
        /// </summary>
        Purchased,

        /// <summary>
        /// 拒绝配货
        /// </summary>
        RefusePurchase,
        
        待发货,
        待付款,
    }
}
