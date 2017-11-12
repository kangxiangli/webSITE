using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Offices
{
    /// <summary>
    /// APP考勤
    /// </summary>
    public class AppAtten
    {

        public AppAtten()
        {
            ArrivalEarlyCount = 0;
            LeaveLateCount = 0;
            LateCount = 0;
            LeaveEarlyCount = 0;
            NoSignOutCount = 0;
        }
        /// <summary>
        /// 请假次数
        /// </summary>
        public int LeaveCount { get; set; }

        /// <summary>
        /// 加班次数
        /// </summary>
        public int OvertimeCount { get; set; }

        /// <summary>
        /// 外勤次数
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// 补卡次数
        /// </summary>
        public int RepairCount { get; set; }

        /// <summary>
        /// 缺勤次数
        /// </summary>
        public int AbsenceCount { get; set; }

        /// <summary>
        /// 未签退次数
        /// </summary>
        public int NoSignOutCount { get; set; }

        /// <summary>
        /// 迟到次数
        /// </summary>
        public int LateCount { get; set; }


        /// <summary>
        /// 早到次数
        /// </summary>
        public  double ArrivalEarlyCount { get; set; }
        
        /// <summary>
        /// 晚退次数
        /// </summary>
        public  double LeaveLateCount { get; set; }        

        /// <summary>
        /// 早退次数
        /// </summary>
        public  int LeaveEarlyCount { get; set; }               

        /// <summary>
        /// 补班
        /// </summary>
        public double WorkDays { get; set; }

        /// <summary>
        /// 休息
        /// </summary>
        public double RestDays{get;set;}
    }
}
