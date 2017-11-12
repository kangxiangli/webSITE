using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    public enum RetailStatus
    {
        /// <summary>
        /// 已卖出，默认状态，0
        /// </summary>
        正常 = 0,
        /// <summary>
        /// 整单退货，1
        /// </summary>
        整单退货 = 1,
        /// <summary>
        /// 部分退货，2
        /// </summary>
        部分退货 = 2,

        删除 = 3,
        禁用 = 4,
        发货完成 = 5
    }
}
