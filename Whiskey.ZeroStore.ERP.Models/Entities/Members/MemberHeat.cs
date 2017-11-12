using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberHeat : EntityBase<int>
    {
        [Display(Name = "名称")]
        [StringLength(50)]
        [Required]
        public virtual  string HeatName { get; set; }

        [Display(Name = "初始天数")]
        public virtual int? DayStart { get; set; }

        [Display(Name = "终止天数")]
        public virtual int? DayEnd { get; set; }

        [Display(Name = "图标")]
        [StringLength(200)]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
