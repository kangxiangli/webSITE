



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
    public class SaleItem : EntityBase<int>
    {
        public SaleItem() {
			
        }

        [Display(Name = "发货店铺")]
        public virtual int DeliverId { get; set; }

        [Display(Name = "收货店铺")]
        public virtual int ReceiverId { get; set; }

        [Display(Name = "所属商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "所属销售")]
        public virtual int SaleId { get; set; }

        [Display(Name = "销售单号")]
        public virtual string SaleNumber { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual decimal TagPrice { get; set; }

        [Display(Name = "零售价格")]
        public virtual decimal RetailPrice { get; set; }

        [Display(Name = "批发价格")]
        public virtual decimal WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        public virtual decimal PurchasePrice { get; set; }

        [Display(Name = "销售数量")]
        public virtual int Quantity { get; set; }



        [ForeignKey("DeliverId")]
        public virtual Store Deliver { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Store Receiver { get; set; }

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


    }
}


