using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{
    /// <summary>
    /// 盘点状态
    /// </summary>
    public enum CheckstatFlag
    {
        /// <summary>
        /// 未盘点
        /// </summary>
        NotCheck,

        /// <summary>
        /// 盘点中
        /// </summary>
        Checking,

        /// <summary>
        /// 盘点完成
        /// </summary>
        Checked,
    }
}
