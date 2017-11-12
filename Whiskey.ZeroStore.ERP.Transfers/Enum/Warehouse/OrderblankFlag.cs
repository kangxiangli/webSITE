using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse
{
    /// <summary>
    /// 配货标识
    /// </summary>
    public enum OrderblankFlag
    {
        /// <summary>
        /// 采购单到配货单
        /// </summary>
        Purchase,

        /// <summary>
        /// 直接创建配货单
        /// </summary>
        Orderblank,
    }
}
