
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
    public class Color : EntityBase<int>
    {
        public Color() {
            ColorName = "";
            ColorCode = "";
            //Children = new List<Color>();
            Products = new List<Product>();
        }

        //[Display(Name = "父级色彩")]
        //public virtual int? ParentId { get; set; }

        [Display(Name = "色彩名称")]
        [Required]
        [StringLength(10)]
        public virtual string ColorName { get; set; }

        [Display(Name = "色彩编码")]
        [StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string ColorCode { get; set; }         

        [Display(Name = "色彩描述")]
        [StringLength(120)]         
        public virtual string Description { get; set; }

        #region 注释字段

        //[Display(Name = "最小色相值")]
        //public virtual double MinHue { get; set; }

        //[Display(Name = "最大色相值")]
        //public virtual double MaxHue { get; set; }

        //[Display(Name = "最小饱和度")]
        //public virtual double MinSaturation { get; set; }

        //[Display(Name = "最大饱和度")]
        //public virtual double MaxSaturation { get; set; }

        //[Display(Name = "最小明度值")]
        //public virtual double MinLightness { get; set; }

        //[Display(Name = "最大明度值")]
        //public virtual double MaxLightness { get; set; }

        //[Display(Name = "RGB")]
        //[StringLength(30)]
        //public virtual string RGB { get; set; }

        //[Display(Name = "HSL")]
        //[StringLength(30)]
        //public virtual string HSL { get; set; }

        #endregion
        
        [Display(Name ="色值")]
        [StringLength(20)]
        public virtual string ColorValue { get; set; }

        [Display(Name = "颜色图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        //[ForeignKey("ParentId")]
        //public virtual Color Parent { get; set; }

        //public virtual ICollection<Color> Children { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public virtual ICollection<Gallery> Galleries { get; set; }

    }
}


