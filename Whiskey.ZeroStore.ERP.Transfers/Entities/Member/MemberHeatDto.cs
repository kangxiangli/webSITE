using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberHeatDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "名称")]
        [StringLength(50)]
        [Required]
        public virtual string HeatName { get; set; }

        [Display(Name = "初始天数")]
        public virtual int? DayStart { get; set; }

        [Display(Name = "终止天数")]
        public virtual int? DayEnd { get; set; }

        [Display(Name = "图标")]
        [StringLength(200)]
        public virtual string IconPath { get; set; }

    }
}
