using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Whiskey.Utility.Data
{
    /// <summary>
    /// 锁定库存类型
    /// </summary>
    public enum LockInventsType
    { 
        /// <summary>
        /// 采购锁定
        /// </summary>
        [Description("采购锁定")]
        Purchase,
        /// <summary>
        /// 配货锁定
        /// </summary>
        [Description("配货锁定")]
        Delivery

    }
}
