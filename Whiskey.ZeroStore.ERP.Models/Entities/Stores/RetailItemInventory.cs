using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    ///[零售明细库存表] 
    ///记录商品零售订单每个明细下关联的库存流水号
    /// </summary>
    [Serializable]
    public class RetailInventory:EntityBase<int>
    {
        public int? RetailItemId { get; set; }

        public int? InventoryId { get; set; }


        /// <summary>
        /// [冗余]零售订单号
        /// </summary>
        [Index(IsClustered = false)]
        [MaxLength(50)]
        public string RetailNumber { get; set; }

        /// <summary>
        /// [冗余]商品条码
        /// </summary>
        [Index(IsClustered = false)]
        [StringLength(18)]
        public string ProductBarcode { get; set; }

        

        [ForeignKey("RetailItemId")]
        public virtual RetailItem RetailItem { get; set; }


        [ForeignKey("InventoryId")]
        public virtual Inventory Inventory { get; set; }

    }
}
