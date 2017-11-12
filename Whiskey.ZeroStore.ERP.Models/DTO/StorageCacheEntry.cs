using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class StorageCacheEntry
    {
        public int Id { get; set; }

        [Display(Name = "所属店铺")]
        public int StoreId { get; set; }

        [Display(Name = "仓库类型")]
        public int StorageType { get; set; }//0:线上，1:线下
        /// <summary>
        /// 0：可零售商品仓库  1：设备仓库(不可零售商品)
        /// </summary>
        [Display(Name = "仓库种类")]
        public int StorageCategory { get; set; }

        [Display(Name = "仓库名称")]
        public string StorageName { get; set; }
        [Display(Name = "联系电话")]
        public string TelePhone { get; set; }
        [Display(Name = "仓库地址")]
        public string StorageAddress { get; set; }

        [Display(Name = "作为会员归属库")]
        public bool IsVestIn { get; set; }
        [Display(Name = "作为订购库")]
        public bool IsOrderStorage { get; set; }
        [Display(Name = "作为默认仓库")]
        public bool IsDefaultStorage { get; set; }

        [Display(Name = "统计信息公开")]
        public bool IsPublicInfo { get; set; }

        [Display(Name = "仓库简介")]
        public string Description { get; set; }
        /// <summary>
        /// 盘点锁定
        /// </summary>
        [Display(Name = "盘点锁定")]
        public bool CheckLock { get; set; }
    }
}
