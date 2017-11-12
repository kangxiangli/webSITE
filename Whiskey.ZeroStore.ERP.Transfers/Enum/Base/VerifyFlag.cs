using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Base
{
    public enum VerifyFlag
    {
        /// <summary>
        /// 审核中
        /// </summary>
        [Description("审核中")]
        Verifing,

        /// <summary>
        /// 审核通过
        /// </summary>
        [Description("审核通过")]
        Pass,

        /// <summary>
        /// 审核不通过
        /// </summary>
        [Description("审核不通过")]
        NoPass,

        /// <summary>
        /// 待确认
        /// </summary>
        [Description("待确认")]
        Waitting,
    }

}
