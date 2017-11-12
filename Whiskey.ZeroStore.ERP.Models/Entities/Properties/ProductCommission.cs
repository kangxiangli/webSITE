using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class ProductCommission : EntityBase<int>
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
        public virtual int StoreId { get; set; }

        [Display(Name = "品牌")]
        public virtual int BrandId { get; set; }

        [Display(Name = "季节")]
        public virtual int SeasonId { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("SeasonId")]
        public virtual Season Season { get; set; }
              
    }
}
