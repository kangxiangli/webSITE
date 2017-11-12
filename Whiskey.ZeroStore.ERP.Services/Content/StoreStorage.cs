using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    public class StoreSelectItem
    {
        public StoreSelectItem()
        {
            Storages = new List<StorageSelectItem>();
        }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        /// <summary>
        /// 店铺下的所有仓库
        /// </summary>
        public List<StorageSelectItem> Storages { get; set; }
    }
    public class StorageSelectItem
    {
        public int StorageId { get; set; }
        public string StorageName { get; set; }
    }
}
