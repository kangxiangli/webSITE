
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
    public class RestoredItem : EntityBase<int>
    {
        public RestoredItem() {

        }


        [Display(Name = "发货店铺")]
        public virtual int SenderId { get; set; }

        [Display(Name = "收货店铺")]
        public virtual int ReceiverId { get; set; }

        [Display(Name = "关联商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "关联返货")]
        public virtual int RestoreId { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual decimal TagPrice { get; set; }

        [Display(Name = "零售价格")]
        public virtual decimal RetailPrice { get; set; }

        [Display(Name = "批发价格")]
        public virtual decimal WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        public virtual decimal PurchasePrice { get; set; }

        [Display(Name = "返货数量")]
        public virtual int Quantity { get; set; }


        [ForeignKey("SenderId")]
        public virtual Store Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Store Receiver { get; set; }

        [ForeignKey("RestoreId")]
        public virtual Restored Restored { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


