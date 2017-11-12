



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 退货记录明细
    /// </summary>
	[Serializable]
    public class ReturnedItem : EntityBase<int>
    {
        /// <summary>
        /// 退货单id
        /// </summary>
        public virtual int? ReturnedId { get; set; }
        /// <summary>
        /// 退货单号
        /// </summary>
        [StringLength(50)]
        [Index(IsClustered = false)]
        public string ReturnedNumber { get; set; }
        /// <summary>
        /// 零售订单id
        /// </summary>
        public virtual int? RetailId { get; set; }
        /// <summary>
        /// 零售订单单号
        /// </summary>
        [StringLength(50)]
        [Index(IsClustered = false)]
        public string RetailNumber { get; set; }


        /// <summary>
        /// 退货库存id
        /// </summary>
        public virtual int? InventoryId { get; set; }

        /// <summary>
        /// 库存流水号
        /// </summary>
        public string ProductBarcode { get; set; }

        /// <summary>
        /// 是否已退
        /// </summary>
        public bool IsReturn { get; set; }




        /// <summary>
        /// 固定为1,每条明细记录关联1条退货的商品库存
        /// </summary>
        [Display(Name = "退货数量")]
        public virtual int Quantity { get; set; }


        [Display(Name = "零售价格")]
        public virtual decimal RetailPrice { get; set; }


        

        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }


        [ForeignKey("ReturnedId")]
        public virtual Returned Returned { get; set; }
        [ForeignKey("RetailId")]
        public virtual Retail Retail { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


    }
}


