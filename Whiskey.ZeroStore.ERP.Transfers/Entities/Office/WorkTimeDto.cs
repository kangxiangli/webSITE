using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class WorkTimeDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "工作时间名称")]
        [Required(ErrorMessage="名称不能为空")]
        [StringLength(15)]
        public virtual string WorkTimeName { get; set; }

        [Display(Name = "工作时间类型")]
        public virtual int WorkTimeType { get; set; }

        //0 职位时间 1 个人时间
        [Display(Name = "时间类型")]
        public virtual int TimeType { get; set; }

        //是否启用 个人时间
        public virtual bool IsPersonalTime { get; set; }

        [Display(Name = "是否享有公休假")]
        public virtual bool IsVacations { get; set; }

        [Display(Name = "弹性工作时间")]
        [Required(ErrorMessage = "是否弹性工作时间")]
        public virtual bool IsFlexibleWork { get; set; }

        [Display(Name = "上班时间")]
        [StringLength(10)]
        [Required(ErrorMessage = ("上班时间不能为空"))]
        public virtual string AmStartTime { get; set; }

        [Display(Name = "上午下班时间")]
        [StringLength(10)]
        [Required(ErrorMessage = ("下班时间不能为空"))]
        public virtual string AmEndTime { get; set; }

        [Display(Name = "下午上班时间")]
        [StringLength(10)]
        [Required(ErrorMessage = ("上班时间不能为空"))]
        public virtual string PmStartTime { get; set; }

        [Display(Name = "下班时间")]
        [StringLength(10)]
        [Required(ErrorMessage = ("下班时间不能为空"))]
        public virtual string PmEndTime { get; set; }

        [Display(Name = "工作周")]
        [StringLength(500)]
        [Required(ErrorMessage = ("工作周不能为空"))]
        public virtual string WorkWeek { get; set; }

        [Display(Name = "工作时长")]
        [Required(ErrorMessage = ("工作时长不能为空"))]
        public virtual int WorkHour { get; set; }

        [Display(Name = "简介")]
        [StringLength(120)]
        public virtual string Summary { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }

        public virtual bool IsEnabled { get; set; }

        [Display(Name ="用户ID")]
        public int? AdminId { get; set; }
    }
}
