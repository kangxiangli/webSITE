using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class ProductAttribute : EntityBase<int>
    {
        public ProductAttribute() {
            AttributeName = "";
            AttributeLevel = 0;
            Description = "";
            ProductOrigNumbers = new List<ProductOriginNumber>();
            ProductAttributeImage = new List<ProductAttributeImage>();
            Children = new List<ProductAttribute>();
        }

        [Display(Name = "父级属性")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "属性名称")]
        [StringLength(10)]
        [Required]
        public virtual string AttributeName { get; set; }

        [Display(Name = "编码")]
        [StringLength(10)]
        public virtual string CodeNum { get; set; }

        [Display(Name = "属性层级")]
        public virtual int AttributeLevel { get; set; }

        [Display(Name = "属性描述")]
        [StringLength(120)]
        public virtual string Description { get; set; }

        [Display(Name = "属性图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("ParentId")]
        public virtual ProductAttribute Parent { get; set; }

        public virtual ICollection<ProductOriginNumber> ProductOrigNumbers { get; set; }

        public virtual ICollection<ProductAttribute> Children { get; set; }
        /// <summary>
        /// 风格图片
        /// </summary>
        public virtual ICollection<ProductAttributeImage> ProductAttributeImage { get; set; }

    }
}


