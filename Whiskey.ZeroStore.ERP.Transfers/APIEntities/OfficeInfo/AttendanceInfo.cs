using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers.OfficeInfo
{    

    public class AttendanceInfo
    {

        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 员工真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 迟到次数
        /// </summary>
        public int LateCount { get; set; }
        
        /// <summary>
        /// 早退次数
        /// </summary>
        public int LeaveEarlyCount { get; set; }
        
        /// <summary>
        /// 未签退次数
        /// </summary>
        public int NoSignOutCount { get; set; }

        /// <summary>
        /// 请假天数
        /// </summary>
        public double LeaveDays { get; set; }

        /// <summary>
        /// 加班天数
        /// </summary>
        public double OvertimeDays { get; set; }

        /// <summary>
        /// 外勤天数
        /// </summary>
        public double FieldDays { get; set; }

        /// <summary>
        /// 旷工次数
        /// </summary>
        public double AbsenceDays{get;set;}

        /// <summary>
        /// 正常上班天数
        /// </summary>
        public double NormalDays { get; set; }
        
        /// <summary>
        /// 绩效分钟数
        /// </summary>
        public double Minutes { get; set; }

        /// <summary>
        /// 绩效天数
        /// </summary>
        public double Days { get; set; }


        /// <summary>
        /// 签到
        /// </summary>
        /// <remarks>-1:未签到；0：正常；1：补卡审核成功</remarks>
        public int IsSign { get; set; }

        /// <summary>
        /// 签退
        /// </summary>
        /// <remarks>-1:未签退；0：正常；1：补卡审核成功</remarks>
        public int IsSignOut { get; set; }

        /// <summary>
        /// 迟到
        /// </summary>
        /// <remarks>-1:迟到；0：正常；1：补卡审核成功</remarks>
        public int IsLate { get; set; }

        /// <summary>
        /// 早退
        /// </summary>
        /// <remarks>-1:早退；0：正常；1：补卡审核成功</remarks>
        public int IsLeaveEarly { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 考勤信息
        /// </summary>
        public ICollection<AttendanceInfo> Children { get; set; }
    }
 
}
