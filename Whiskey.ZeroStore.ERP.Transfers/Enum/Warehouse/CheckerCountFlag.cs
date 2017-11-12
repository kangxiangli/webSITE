using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{
    /// <summary>
    /// 仓库数量类型
    /// </summary>
    public enum  CheckerCountFlag
    {
        /// <summary>
        /// 待盘
        /// </summary>
        Check,

        /// <summary>
        /// 已盘
        /// </summary>
        Checked,

        /// <summary>
        /// 有效
        /// </summary>
        Valid,

        /// <summary>
        /// 无效
        /// </summary>
        Invalid,

        /// <summary>
        /// 缺货
        /// </summary>
        Missing,

        /// <summary>
        /// 余货
        /// </summary>
        Residue,
    }
}
