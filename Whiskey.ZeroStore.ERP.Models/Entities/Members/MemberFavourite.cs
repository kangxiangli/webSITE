
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
    public class MemberFavourite : EntityBase<int>
    {
        public MemberFavourite() {

        }

        [Display(Name = "所属会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "收藏商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "吊牌价格")]
        public virtual decimal TagPrice { get; set; }

        [Display(Name = "零售价格")]
        public virtual decimal RetailPrice { get; set; }

        [Display(Name = "批发价格")]
        public virtual decimal WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        public virtual decimal PurchasePrice { get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


