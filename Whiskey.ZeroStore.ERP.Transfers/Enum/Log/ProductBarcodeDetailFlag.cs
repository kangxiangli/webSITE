using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Log
{
    /// <summary>
    /// ProductBarcodeDetai Status的说明
    /// </summary>    
    public enum ProductBarcodeDetailFlag
    {
        /// <summary>
        /// 未使用
        /// </summary>
        Unused,

        /// <summary>
        /// 已入库
        /// </summary>
        AddStorage,

        /// <summary>
        /// 废除
        /// </summary>
        Abolish,
    }
}
