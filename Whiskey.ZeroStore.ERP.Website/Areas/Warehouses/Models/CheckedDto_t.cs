using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class CheckedDto_t
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string StoreName { get; set; }
        public string StorageName { get; set; }
        public string CheckerName { get; set; }
        public int CheckCount { get; set; }

        public string BrandName { get; set; }

        public string CategoryName { get; set; }
        public int CheckedQuantity { get; set; }
        public int BeforeCheckQuantity { get; set; }
        public int CheckedCount { get; set; }
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }

        public int ResidueCount { get; set; }
        public int MissingCount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public virtual CheckerFlag CheckerState { get; set; }//1:盘点中2：中断 3：完成
        public string AdminName { get; set; }

        /// <summary>
        /// 是否为第一个
        /// </summary>
        public bool IsIndex { get; set; }

        //总盘点商品数量
        public int Quantity { get; set; }
    }
}