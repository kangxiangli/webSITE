using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ProductCommissionDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "提成名称")]
        [Required(ErrorMessage = "提成名称不能为空")]
        [StringLength(12, ErrorMessage = ("提成名称长度在0～12字符内"))]
        public string CommissionName { get; set; }

        [Display(Name = "提成百分比")]
        [Required]
        [Range(0.000001, 1, ErrorMessage = "折扣范围在0.000001~~1之间")]
        public virtual double Percentage { get; set; }

        [Display(Name = "店铺")]
        [Required]
        public virtual int StoreId { get; set; }

        [Display(Name = "店铺名称")]
        [Required]
        public virtual string StoreName { get; set; }

        [Display(Name = "品牌")]
        [Required]
        public virtual int BrandId { get; set; }

        [Display(Name = "品牌名称")]
        [Required]
        public virtual string BrandName { get; set; }

        [Display(Name = "季节")]
        [Required]
        public virtual int SeasonId { get; set; }

        [Display(Name = "季节名称")]
        [Required]
        public virtual string SeasonName { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
