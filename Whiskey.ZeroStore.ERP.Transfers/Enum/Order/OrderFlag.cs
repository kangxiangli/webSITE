using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Order
{
    public enum OrderFlag
    {
        /// <summary>
        /// 未支付
        /// </summary>
        Unpaid,

        /// <summary>
        /// 已支付
        /// </summary>
        Paid,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled
    }
}
