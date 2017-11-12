using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Base
{
    public enum ExpressFlag
    {
        /// <summary>
        /// 提交订单
        /// </summary>
        SubmitOrder,
        /// <summary>
        /// 商品出库
        /// </summary>
        Delivery,
        /// <summary>
        /// 等待收货
        /// </summary>
        WaitReceipt,

        /// <summary>
        /// 确认收货
        /// </summary>
        Confirm,
    }
}
