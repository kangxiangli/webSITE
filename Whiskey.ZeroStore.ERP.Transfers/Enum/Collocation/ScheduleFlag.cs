using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Collocation
{
    /// <summary>
    /// 进度
    /// </summary>
    public enum ScheduleFlag
    {
        /// <summary>
        /// 抢订中
        /// </summary>
        Grabing,

        /// <summary>
        /// 完成中
        /// </summary>
        Completing,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 过期
        /// </summary>
        Expired,
    }
}
