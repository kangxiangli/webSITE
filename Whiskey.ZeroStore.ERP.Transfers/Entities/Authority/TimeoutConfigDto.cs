using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class TimeoutConfigDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "任务名称")]
        [Required(ErrorMessage = "不能为空"), StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string TaskName { get; set; }

        [Display(Name = "执行路径")]
        [Required(ErrorMessage = "不能为空")]
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
    }
}
