using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class BarCodeConfig : EntityBase<int>
    {
        [Display(Name = "打印机纸带宽度")]
        [Required]
        [DefaultValue(PrinterPaperType._30_80)]
        public virtual PrinterPaperType PrinterPaperType { get; set; }

        [Display(Name = "默认品牌")]
        [Required]
        [DefaultValue(true)]
        public virtual bool IsDefaultBrand { get; set; }

        [Display(Name = "DIY品牌")]
        [StringLength(20)]
        public virtual string DIYBrand { get; set; }

        /// <summary>
        /// 纸张方向
        /// </summary>
        [Display(Name = "纸张方向")]
        [DefaultValue(PrinterPaperDirection._横版)]
        public virtual PrinterPaperDirection PrinterPaperDirection { get; set; }

        //[Display(Name = "启用品牌价格")]
        //[Required]
        //[DefaultValue(true)]
        //public virtual bool IsDefaultBrandPrice { get; set; }
    }
}
