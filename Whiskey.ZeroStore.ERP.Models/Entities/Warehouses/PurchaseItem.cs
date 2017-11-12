



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class PurchaseItem : EntityBase<int>
    {
        public PurchaseItem()
        {
            PurchaseItemProducts = new List<PurchaseItemProduct>();
        }
        [Display(Name = "所属商品")]
        public virtual int? ProductId { get; set; }

        [Display(Name = "所属采购单")]
        public virtual int? PurchaseId { get; set; }
       
        [Display(Name = "吊牌价格")]
        public virtual float TagPrice { get; set; }

        [Display(Name = "批发价格")]
        public virtual float WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        public virtual float PurchasePrice { get; set; }

        [Display(Name = "采购数量")]
        public virtual int Quantity { get; set; }

        [Display(Name = "是否编辑")]
        [Description("在审核采购单时，该采购信息是否发生了更改")]
        public virtual bool IsEdit { get; set; }

        /// <summary>
        /// 在采购单进行配货时内部调整添加的
        /// </summary>
        [Display(Name = "新添加")]
        public virtual bool IsNewAdded { get; set; }

        /// <summary>
        /// 采购商品对应的条码，如：，SDFA24，43242F,4324FSA,
        /// </summary>
        //public virtual string Barcodes { get; set; }

        [ForeignKey("PurchaseId")]
        public virtual Purchase Purchase { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


        public virtual ICollection<PurchaseItemProduct> PurchaseItemProducts { get; set; }
    }
}


