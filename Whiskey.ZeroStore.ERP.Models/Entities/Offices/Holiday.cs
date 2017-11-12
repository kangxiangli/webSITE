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
    public class Holiday : EntityBase<int>
    {
         
        [DisplayName("休假名称")]
        [Required(ErrorMessage="休假名称不能为空")]
        [StringLength(10,ErrorMessage="最大长度不能超过{1}")]
        public virtual string HolidayName { get; set; }

        [DisplayName("开始时间")]
        public virtual DateTime StartTime { get; set; }

        [DisplayName("结束时间")]
        public virtual DateTime EndTime { get; set; }
        public virtual string MakeupclassStartTime { get; set; }
        public virtual string MakeupclassEndTime { get; set; }
        [DisplayName("工作日期")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}")]
        public virtual string WorkDates { get; set; }

        [DisplayName("休息天数")]        
        public virtual int HolidayDays { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
