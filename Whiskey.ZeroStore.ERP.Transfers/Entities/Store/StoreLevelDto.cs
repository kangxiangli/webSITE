using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class StoreLevelDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "等级名称")]
        [Required(ErrorMessage = "不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string LevelName { get; set; }

        [Display(Name = "升级条件")]
        public virtual float UpgradeCondition { get; set; }

        [Display(Name = "等级折扣")]
        [Range(0, 1)]
        public virtual float Discount { get; set; }

        [Display(Name = "图标")]
        [StringLength(200)]
        public virtual string IconPath { get; set; }
    }
}
