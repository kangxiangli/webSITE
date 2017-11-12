using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Office
{
    /// <summary>
    /// 考勤状态
    /// </summary>
    public enum AttendanceFlag
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal,
        
        /// <summary>
        /// 上午缺勤
        /// </summary>
        AmAbsence,
        
        /// <summary>
        /// 下午缺勤
        /// </summary>
        PmAbsence,

        /// <summary>
        ///缺勤一天
        /// </summary>
        DayOfAbsence,

        /// <summary>
        /// 上午请假
        /// </summary>
        AmLeave,

        /// <summary>
        /// 下午请假
        /// </summary>
        PmLeave,

        /// <summary>
        ///请假一天
        /// </summary>
        DayOfLeave,

        /// <summary>
        /// 上午外勤
        /// </summary>
        AmField,

        /// <summary>
        /// 下午外勤
        /// </summary>
        PmField,

        /// <summary>
        ///外勤一天
        /// </summary>
        DayOfField,

        /// <summary>
        /// 上午加班
        /// </summary>
        AmOvertime,

        /// <summary>
        /// 下午加班
        /// </summary>
        PmOvertime,

        /// <summary>
        ///加班一天
        /// </summary>
        DayOfOvertime,

        /// <summary>
        ///补卡
        /// </summary>
        FillCard
    }
}
