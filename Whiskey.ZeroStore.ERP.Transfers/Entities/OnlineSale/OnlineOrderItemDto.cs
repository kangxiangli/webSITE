using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OnlineOrderItemDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "线上订单")]
        public virtual int OnlineOrderId { get; set; }

        [Display(Name = "商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "商品数量")]
        public virtual int Quantity { get; set; }

        [Display(Name = "商品价格")]
        public virtual decimal TagPrice { get; set; }

        [Display(Name = "优惠卷")]
        public virtual int? CouponId { get; set; }

        public Int32 Id { get; set; }
    }
}
