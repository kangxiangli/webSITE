using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Office
{
    /// <summary>
    /// 接口考勤标识
    /// </summary>
    public enum ApiAttenFlag
    {
        /// <summary>
        /// 请假
        /// </summary>
        Leave,

        /// <summary>
        /// 加班
        /// </summary>
        Overtime,

        /// <summary>
        /// 外勤
        /// </summary>
        Field,

        /// <summary>
        /// 补卡
        /// </summary>
        Repair,

        /// <summary>
        ///  未签到
        /// </summary>
        Absence,

        /// <summary>
        /// 未签退
        /// </summary>
        NoSignOut,

        /// <summary>
        /// 迟到
        /// </summary>
        Late,

        /// <summary>
        /// 早退
        /// </summary>
        LeaveEarly,

        /// <summary>
        /// 早到
        /// </summary>
        ArrivalEarly,

        /// <summary>
        /// 晚退
        /// </summary>
        LeaveLate
    }
}
