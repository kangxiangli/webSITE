



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.ComponentModel;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Inventory : EntityBase<int>
    {
        public Inventory()
        {

        }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属仓库")]
        public virtual int StorageId { get; set; }

        [Display(Name = "所属商品")]
        public virtual int ProductId { get; set; }

        [Display(Name = "商品编号")]
        [StringLength(15)]
        [Index(IsClustered = false, IsUnique = false)]
        public virtual string ProductNumber { get; set; }
        /// <summary>
        /// 商品的唯一标识，3位36进制
        /// </summary>
        [StringLength(3)]
        public virtual string OnlyFlag { get; set; }
        /// <summary>
        /// 商品的一维码
        /// </summary>
        [StringLength(18)]
        [Index(IsClustered = false, IsUnique = false)]
        public virtual string ProductBarcode { get; set; }


        /// <summary>
        /// 在采购时确定是否锁定库存
        /// </summary>
        [Display(Name = "是否锁定库存")]
        public bool IsLock { get; set; }

        

        /// <summary>
        /// 目前没有使用，暂且保留
        /// </summary>
        [Display(Name = "库位编码")]
        [StringLength(20)]
        public virtual string LocationCode { get; set; }


        /// <summary>
        /// 具体见InventoryStatus
        /// </summary>
        public InventoryStatus Status { get; set; }



        [Display(Name = "备注")]
        [StringLength(100)]
        public virtual string Description { get; set; }
        /// <summary>
        /// 32位guid值
        /// </summary>
        [StringLength(36)]
        public string ProductLogFlag { get; set; }

        public int? InventoryRecordId { get; set; }

        [StringLength(20)]
        public virtual string Others { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage Storage { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("InventoryRecordId")]
        public virtual InventoryRecord InventoryRecord { get; set; }

        public virtual ICollection<ProductOperationLog> ProductOperationLogs { get; set; }



    }

    [Serializable]
    public class LockInventoryDto
    {
        public int OperatorId { get; set; }
        public string ProductBarcode { get; set; }
    }
}


