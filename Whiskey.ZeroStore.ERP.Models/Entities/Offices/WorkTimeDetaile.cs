using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkTimeDetaile : EntityBase<int>
    {

        public virtual int? WorkTimeId { get; set; }


        [Display(Name = "工作时间类型")]
        public virtual int WorkTimeType { get; set; }
        [Display(Name = "上午上班时间")]
        [StringLength(10)]
        public virtual string AmStartTime { get; set; }
        [Display(Name = "上午下班时间")]
        [StringLength(10)]
        public virtual string AmEndTime { get; set; }
        [Display(Name = "下午上班时间")]
        [StringLength(10)]
        public virtual string PmStartTime { get; set; }
        [Display(Name = "下午下班时间")]
        [StringLength(10)]
        public virtual string PmEndTime { get; set; }
        [Display(Name = "工作时长")]
        public virtual int WorkHour { get; set; }
        [Display(Name = "年份")]
        public virtual int Year { get; set; }
        [Display(Name = "月份")]
        public virtual int Month { get; set; }
        [Display(Name = "工作日")]
        public virtual int WorkDay { get; set; }
        [Display(Name = "是否弹性工作")]
        public virtual bool IsFlexibleWork { get; set; }
        [ForeignKey("WorkTimeId")]
        public virtual WorkTime WorkTime { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
