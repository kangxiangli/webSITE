
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class ProductCrowd : EntityBase<int>
	{
		[Display(Name = "面向人群")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string CrowdName { get; set; }

        [Display(Name = "人群编码")]
        [StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string CrowdCode { get; set; }

        [Display(Name = "人群描述")]
        [StringLength(120)]
        public virtual string Description { get; set; }

        [Display(Name = "图标")]
        [StringLength(200)]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

