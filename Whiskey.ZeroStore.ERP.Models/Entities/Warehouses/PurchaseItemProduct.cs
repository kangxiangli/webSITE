



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
    public class PurchaseItemProduct : EntityBase<int>
    {
        public PurchaseItemProduct()
        {			
        }

        [DisplayName("采购单详情")]        
        public virtual int? PurchaseItemId { get; set; }
                
        [Description("记录每个商品的条形码")]
        [StringLength(20)]
        public virtual string ProductBarcode { get; set; }                 

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("PurchaseItemId")]
        public virtual PurchaseItem PurchaseItem { get; set; }
    }
}


