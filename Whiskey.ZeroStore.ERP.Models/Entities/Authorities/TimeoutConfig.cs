using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class TimeoutConfig : EntityBase<int>
    {
        [Display(Name = "任务名称")]
        [Required, StringLength(50)]
        public virtual string TaskName { get; set; }

        [Display(Name = "执行路径")]
        [Required]
        public virtual string ActionPath { get; set; }

        [Display(Name = "超时的天数")]
        [Required]
        public virtual int TimeoutDay { get; set; }

        [Display(Name = "超时的小时")]
        [Required]
        public virtual int TimeoutHour { get; set; }

        [Display(Name = "超时的分钟")]
        [Required]
        public virtual int TimeoutMinute { get; set; }

        [Display(Name = "超时的秒")]
        [Required]
        public virtual int TimeoutSecond { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
