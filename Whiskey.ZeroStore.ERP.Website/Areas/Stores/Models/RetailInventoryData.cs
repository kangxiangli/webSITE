using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class RetailInventoryData
    {
        public int ProductId { get; set; }
        /// <summary>
        /// 库存id
        /// </summary>
        public int Id { get; set; }
        public string ProductBarcode { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public float TagPrice { get; set; }
        public float RetailPrice { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }

        public string IconPath { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public string SeasonName { get; set; }
        public string ThumbnailPath { get; set; }
        public string StoreName { get; set; }
        public int StoreId { get; set; }
        public float RetailDiscount { get; set; }
    }
}