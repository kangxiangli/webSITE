using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 请假信息模块
    /// </summary>
    public class LeaveInfo : EntityBase<int>
    {
        [DisplayName("请假人")]
        public virtual int AdminId { get; set; }

        [DisplayName("请假原因")]
        [StringLength(25)]
        public virtual string LeaveReason { get; set; }

        [DisplayName("请假开始时间")]
        public virtual DateTime StartTime { get; set; }

        [DisplayName("请假结束时间")]
        public virtual DateTime EndTime { get; set; }

        [DisplayName("请假天数")]
        public virtual double LeaveDays { get; set; }

        [DisplayName("休息时间")]
        [Required(ErrorMessage = "休息时间")]
        public virtual double RestHours { get; set; }

        [DisplayName("请假方式")]
        public virtual int LeaveMethod { get; set; }

        [DisplayName("使用年假")]
        public virtual double UseAnnualLeaveDay { get; set; }

        [DisplayName("年假具体时间（1-:非年假或带薪休假；0：全天；1：上午；2：下午）")]
        [DefaultValue(-1)]
        public virtual int AmOrPm { get; set; }

        [DisplayName("年假天数")]
        public virtual double AnnualLeaveDays { get; set; }

        [DisplayName("带薪休假天数")]
        public virtual double PaidLeaveDays { get; set; }

        [DisplayName("调休天数")]
        public virtual double ChangeRestDays { get; set; }

        [DisplayName("休假类型")]
        public virtual int VacationType { get; set; }

        [DisplayName("审核状态")]
        public virtual int VerifyType { get; set; }

        [DisplayName("审核人")]
        public virtual int? VerifyAdminId { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        [ForeignKey("VerifyAdminId")]
        public virtual Administrator VerifyAdmin { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }

        [DisplayName("该请假申请所扣除积分数量")]
        [DefaultValue(0.00)]
        public virtual decimal DeductionLeavePoints { get; set; }
    }
}
