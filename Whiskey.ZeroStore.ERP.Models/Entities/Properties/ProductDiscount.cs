using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class ProductDiscount : EntityBase<int>
    {
        public ProductDiscount()
        {
            CreatedTime = UpdatedTime = StartTime = EndTime = DateTime.Now;
        }
        [Display(Name = "折扣名称")]
        [Required(ErrorMessage = "折扣名称不能为空")]
        [StringLength(50, ErrorMessage = ("折后名称长度在0～12字符内"))]
        public string DiscountName { get; set; }

        [Display(Name = "零售折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual float RetailDiscount { get; set; }

        [Display(Name = "批发折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual float WholesaleDiscount { get; set; }
        [Display(Name = "采购折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual float PurchaseDiscount { get; set; }

        /// <summary>
        ///1：大款号 2:商品 3：复杂筛选
        /// </summary>
        public virtual int DiscountType { get; set; }
        [Display(Name = "折扣编号")]
        public string DiscountCode { get; set; }
        [StringLength(100)]
        public virtual string Description { get; set; }
        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }
        [Display(Name = "结束时间")]
        public virtual DateTime EndTime { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public int BrandCount { get; set; }
        public int ProductOrigNumberCount { get; set; }
        public virtual ICollection<Brand> Brands { get; set; }
        public virtual ICollection<ProductOriginNumber> ProductOrigNumbers { get; set; }

    }
}
