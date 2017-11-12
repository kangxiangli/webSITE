using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class StorageDto_t
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public int StorageType { get; set; }
        public string StorageName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEnabled { get; set; }
        public int Sequence { get; set; }
        public DateTime UpdatedTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public string AdminName { get; set; }
    }
}