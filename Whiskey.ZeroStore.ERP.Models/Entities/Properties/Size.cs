
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;


namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Size : EntityBase<int>
    {
        public Size() {
            SizeName = "";
            SizeCode = "";            
            Description = "";
            Products = new List<Product>();
            //Children = new List<Size>();
        }


        [Display(Name = "尺码名称")]
        [StringLength(10)]
        public virtual string SizeName { get; set; }

        [Display(Name = "尺码编码")]
        [StringLength(6, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string SizeCode { get; set; }

        [Display(Name = "图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [Display(Name = "尺码描述")]
        [StringLength(120)]
        public virtual string Description { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [Display(Name = "品类名称")]
        public virtual int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [Display(Name = "尺码属性")]
        public virtual int? SizeExtentionId { get; set; }

        [ForeignKey("SizeExtentionId")]
        public virtual SizeExtention SizeExtention { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}


