using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class InventoryDto_t
    {//ProduId:22,StorCou:120,StoreId:12,StorageId:2
        /// <summary>
        /// 商品id
        /// </summary>
        public int ProduId { get; set; }
        /// <summary>
        /// 库存数量
        /// </summary>
        public int StorCou { get; set; }
        /// <summary>
        /// 店铺id
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 仓库id
        /// </summary>
        public int StorageId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Descriptions { get; set; }
    }
    public class Tem_t
    {
        public int Id { get; set; }
        public int Cou { get; set; }

        public string pitemId { get; set; } //采购单明细的ID
    }
}