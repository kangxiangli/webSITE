using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Collocation
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public enum MissionFlag
    {
        /// <summary>
        /// 移除整理
        /// </summary>
        Collocation,

        /// <summary>
        /// 需求购买
        /// </summary>
        Buy,

        /// <summary>
        /// 拼单
        /// </summary>
        CombinedOrders
    }
}
