
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
    public class Restored : EntityBase<int>
    {
        public Restored() {
            RestoredItems = new List<RestoredItem>();
        }


        [Display(Name = "发货店铺")]
        public virtual int? DeliverId { get; set; }

        [Display(Name = "收货店铺")]
        public virtual int? ReceiverId { get; set; }

        [Display(Name = "返货状态")]
        public virtual int Status { get; set; } 

        [Display(Name = "返货单号")]
        public virtual string RestoreNumber { get; set; }


        [ForeignKey("DeliverId")]
        public virtual Store Deliver { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Store Receiver { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<RestoredItem> RestoredItems { get; set; }


    }
}


