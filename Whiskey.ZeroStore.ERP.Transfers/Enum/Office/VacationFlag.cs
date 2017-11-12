using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Office
{
    /// <summary>
    /// 休假类型
    /// </summary>
    public enum VacationFlag
    {        
        /// <summary>
        /// 病假
        /// </summary>
        SickLeave,

        /// <summary>
        /// 事假
        /// </summary>
        Leave,

        /// <summary>
        /// 带薪休假
        /// </summary>
        PaidLeave,

        /// <summary>
        /// 年假
        /// </summary>
        AnnualLeave,

        /// <summary>
        /// 调休
        /// </summary>
        ChangeRest,
    }
}
