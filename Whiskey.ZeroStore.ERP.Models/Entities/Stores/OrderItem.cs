
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class OrderItem : EntityBase<int>
    {
        public OrderItem() {
			
        }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属订单")]
        public virtual int OrderId { get; set; }

        [Display(Name = "所属商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual decimal TagPrice { get; set; }

        [Display(Name = "零售价格")]
        public virtual decimal RetailPrice { get; set; }

        [Display(Name = "商品数量")]
        public virtual int Quantity { get; set; }

        [Display(Name = "获得积分")]
        public virtual int ObtainScore { get; set; }



        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }



    }
}


