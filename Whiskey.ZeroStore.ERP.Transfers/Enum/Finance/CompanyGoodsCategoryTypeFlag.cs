using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Finance
{
    public enum CompanyGoodsCategoryTypeFlag
    {
        /// <summary>
        /// 设备
        /// </summary>
        [Description("设备")]
        Equipment = 1,

        /// <summary>
        /// 耗材
        /// </summary>
        [Description("耗材")]
        Supplies,

        /// <summary>
        /// 工具
        /// </summary>
        [Description("工具")]
        Tool
    }
}
