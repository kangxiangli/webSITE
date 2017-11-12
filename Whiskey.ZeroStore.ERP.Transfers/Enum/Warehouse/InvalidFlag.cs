using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{

    /// <summary>
    /// 针对CheckerItemFlag 无效状态的标识
    /// </summary>
    public enum InvalidFlag
    {
        /// <summary>
        /// 错误编号（系统不存在）
        /// </summary>
        Error,

        /// <summary>
        /// 已经入库使用
        /// </summary>
        Used,
    }
}
