using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;


namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class FieldDto : IAddDto, IEditDto<int>
    {
        [DisplayName("外勤人员")]
        public virtual int AdminId { get; set; }

        [DisplayName("外勤原因")]
        [StringLength(100, ErrorMessage = "外勤原因最大字符长度{0}以内")]
        [Required(ErrorMessage = "外勤原因不能为空")]
        public virtual string FieldReason { get; set; }

        [DisplayName("外勤开始时间")]
        public virtual DateTime StartTime { get; set; }

        [DisplayName("外勤结束时间")]
        public virtual DateTime EndTime { get; set; }

        [DisplayName("外勤时长")]
        public virtual double FieldDays { get; set; }

        [DisplayName("工作天数")]
        public virtual double FieldWorkDays { get; set; }

        [DisplayName("实际工作天数")]
        public virtual double RealWorkDays { get; set; }

        [DisplayName("审核状态")]
        public virtual int VerifyType { get; set; }

        [DisplayName("审核人")]
        public virtual int? VerifyAdminId { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "备注最大字符长度{0}以内")]
        public virtual string Notes { get; set; }

        [DisplayName("标识Id")]
        public virtual int Id { get; set; }
    }
}
