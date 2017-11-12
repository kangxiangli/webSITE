using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{
    /// <summary>
    /// 仓库盘点类型
    /// </summary>
    public enum  CheckerFlag
    {
        /// <summary>
        /// 失败
        /// </summary>
        Fail,

        /// <summary>
        /// 盘点中
        /// </summary>
        Checking,

        /// <summary>
        /// 中断
        /// </summary>
        Interrupt,

        /// <summary>
        /// 盘点完成
        /// </summary>
        Checked,

        /// <summary>
        /// 完成校对
        /// </summary>
        Proofreader


    }
}
