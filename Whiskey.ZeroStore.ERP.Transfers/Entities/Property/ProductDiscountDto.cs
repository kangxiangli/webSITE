using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ProductDiscountDto : IAddDto, IEditDto<int>
    {
          public ProductDiscountDto()
        {
            CreatedTime = UpdatedTime = StartTime = EndTime = DateTime.Now;
        }
        public int Id { get; set; }
        [Display(Name = "折扣名称")]
        [Required(ErrorMessage = "折扣名称不能为空")]
        [StringLength(12, ErrorMessage = ("折后名称长度在0～12字符内"))]
        public string DiscountName { get; set; }

        [Display(Name = "零售折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual float RetailDiscount { get; set; }

        [Display(Name = "批发折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual double WholesaleDiscount { get; set; }
        [Display(Name = "采购折扣")]
        [Required]
        [Range(1, 10, ErrorMessage = "折扣范围在1~10之间")]
        public virtual double PurchaseDiscount { get; set; }
        /// <summary>
        ///  1：大款号 2:商品 3：复杂筛选
        /// </summary>
        public virtual int DiscountType { get; set; }
        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }
        [Display(Name = "结束时间")]
        public virtual DateTime EndTime { get; set; }
        [Display(Name = "折扣编号")]
        public string DiscountCode { get; set; }
        [StringLength(100)]
        public virtual string Description { get; set; }
        public virtual DateTime CreatedTime { get; set; }
        public virtual DateTime UpdatedTime { get; set; }

        public int BrandCount { get; set; }
        public string Brands { get; set; }
        public string BigNumbers { get; set; }

    }
}
