using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Finance
{
    public enum CompanyGoodsCategoryStatusFlag
    {
        /// <summary>
        /// 空闲中
        /// </summary>
        [Description("空闲中")]
        Free,

        /// <summary>
        /// 使用中
        /// </summary>
        [Description("使用中")]
        Use,

        /// <summary>
        /// 已损耗
        /// </summary>
        [Description("已损耗")]
        Loss,

        /// <summary>
        /// 维修中
        /// </summary>
        [Description("维修中")]
        Repair
    }
}
