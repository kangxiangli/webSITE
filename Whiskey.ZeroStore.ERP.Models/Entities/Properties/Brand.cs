
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Brand : EntityBase<int>
    {
        public Brand()
        {
            Products = new List<ProductOriginNumber>();
            Children = new List<Brand>();
        }

        [Display(Name = "父级品牌")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "品牌名称")]
        [Required]
        [StringLength(30)]
        public virtual string BrandName { get; set; }

        [Display(Name = "品牌编码")]
        [StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string BrandCode { get; set; }

        [Display(Name = "品牌图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [Display(Name = "品牌描述")]
        [StringLength(120)]
        public virtual string Description { get; set; }
        [Display(Name="品牌故事")]
        public virtual string BrandStory { get; set; }

        [Display(Name = "默认折扣")]
        public virtual float DefaultDiscount { get; set; }

        [ForeignKey("ParentId")]
        public virtual Brand Parent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Brand> Children { get; set; }

        public virtual ICollection<ProductOriginNumber> Products { get; set; }
        //该品牌涉及到的折扣方案
        public virtual ICollection<ProductDiscount> Discounts { get; set; }
    }
}


