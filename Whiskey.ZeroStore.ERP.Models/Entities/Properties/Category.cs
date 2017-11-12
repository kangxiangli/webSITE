
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
    public class Category : EntityBase<int>
    {
        public Category() {
            CategoryName = "";
            CategoryCode = "";
            //CategoryLevel = 0;
            Description = "";
            Products = new List<ProductOriginNumber>();
            Children = new List<Category>();
        }


        [Display(Name = "父级分类")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "分类名称")]
        [StringLength(10)]
        public virtual string CategoryName { get; set; }

        [Display(Name = "分类编码")]
        [StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string CategoryCode { get; set; }

        //[Display(Name = "分类层级")]
        //public virtual int CategoryLevel { get; set; }

        [Display(Name = "分类描述")]       
        public virtual string Description { get; set; }

        [Display(Name = "图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category Parent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Category> Children { get; set; }

        public virtual ICollection<ProductOriginNumber> Products { get; set; }

        public virtual ICollection<Mission> Missions { get; set; }

        public virtual ICollection<Size> Sizes { get; set; }
    }
}


