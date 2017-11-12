using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{
    /// <summary>
    /// 盘点商品类型
    /// </summary>
    public enum CheckerItemFlag
    {
        /// <summary>
        /// 有效
        /// </summary>
        Valid = 0,

        /// <summary>
        /// 无效
        /// </summary>
        Invalid = 1,

        /// <summary>
        /// 缺货
        /// </summary>
        Lack = 2,

        /// <summary>
        /// 余货
        /// </summary>
        Surplus = 3,
    }
}
