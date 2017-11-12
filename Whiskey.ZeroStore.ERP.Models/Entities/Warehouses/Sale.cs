
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
    public class Sale : EntityBase<int>
    {
        public Sale() {
            SaleItems = new List<SaleItem>();
        }
		

        [Display(Name = "发货店铺")]
        public virtual int? DeliverId { get; set; }

        [Display(Name = "收货店铺")]
        public virtual int? ReceiverId { get; set; }

        [Display(Name = "销售单号")]
        public virtual string PurchaseNumber { get; set; }

        [Display(Name = "销售状态")]
        public virtual int Status { get; set; } 

        [Display(Name = "备注信息")]
        public virtual string Notes { get; set; }


        [Display(Name = "确认发货")]
        public virtual bool IsDelivered { get; set; }

        [Display(Name = "发货操作员")]
        public virtual string DeliverName { get; set; }


        [Display(Name = "确认收货")]
        public virtual bool IsReceived { get; set; }

        [Display(Name = "收货操作员")]
        public virtual string ReceiverName { get; set; }


        [Display(Name = "入库确认")]
        public virtual bool IsStoraged { get; set; }

        [Display(Name = "入库操作员")]
        public virtual string StoragerName { get; set; }



        [ForeignKey("DeliverId")]
        public virtual Store Deliver { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Store Receiver { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }

    }
}


