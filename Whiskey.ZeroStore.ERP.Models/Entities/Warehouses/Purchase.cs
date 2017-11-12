
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using System.ComponentModel;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    [Description("采购店铺")]
    public class Purchase : EntityBase<int>
    {
        public Purchase()
        {
            PurchaseItems = new List<PurchaseItem>();
        }

        /// <summary>
        /// 发货仓库
        /// </summary>
        [Display(Name = "发货仓库")]
        public virtual int?  StorageId { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage Storage { get; set; }

        [Display(Name = "收货店铺")]
        public virtual int? ReceiverId { get; set; }
         
        [Display(Name = "收货仓库")]
        public virtual int? ReceiverStorageId { get; set; }

        [Display(Name = "采购单号")]
        [StringLength(20)]
        [Description("随机生成的编号")]
        public virtual string PurchaseNumber { get; set; }

        [Display(Name = "应付价格")]
        public virtual float OrgPrice { get; set; }

        [Display(Name = "折后价格")]
        public virtual float DespoitPrice { get; set; }

        [Display(Name = "折扣")]
        [Range(0, 1)]
        public virtual float Discount { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public virtual PaymentPurchaseType PaymentType { get; set; }

        /// <summary>
        /// 参考PurchaseStatusFlag
        /// </summary>
        [Display(Name = "采购状态")]
        [Description("参考PurchaseStatusFlag")]
        public virtual int PurchaseStatus { get; set; }
        
        [Description("在审核时是否修改了采购单")]
        public virtual bool IsEdit { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(150)]
        public virtual string Notes { get; set; }

        [Display(Name = "审核备注")]
        [StringLength(150)]
        public virtual string AuditMessage { get; set; }

        [DisplayName("采购来源")]
        public StoreCardOriginFlag OriginFlag { get; set; }

        [DisplayName("购物车")]
        public virtual int? StoreCartId { get; set; }

        [ForeignKey("StoreCartId")]
        public virtual StoreCart StoreCart { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Store Receiver { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
        
        public virtual ICollection<Orderblank> Orderblanks { get; set; }

        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; }

    }
}


