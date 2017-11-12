using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class Storage : EntityBase<int>
    {
        public Storage()
        {
            StoreId = 0;
            StorageType = 1;
            StorageName = "";
            Description = "";
            Inventories = new List<Inventory>();
            //StorageAdmin = new List<Administrator>();
        }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "仓库类型")]
        public virtual int StorageType { get; set; }//0:线上，1:线下
        /// <summary>
        /// 0：可零售商品仓库  1：设备仓库(不可零售商品)
        /// </summary>
        [Display(Name = "仓库种类")]
        public virtual int StorageCategory { get; set; }

        [Display(Name = "仓库名称")]
        public virtual string StorageName { get; set; }
        [Display(Name = "联系电话")]
        public virtual string TelePhone { get; set; }
        [Display(Name = "仓库地址")]
        public virtual string StorageAddress { get; set; }

        [Display(Name = "作为会员归属库")]
        public virtual bool IsVestIn { get; set; }
        [Display(Name = "作为订购库")]
        public virtual bool IsOrderStorage { get; set; }
        [Display(Name = "作为临时仓库")]
        public virtual bool IsTempStorage { get; set; }
        [Display(Name = "作为默认仓库")]
        public virtual bool IsDefaultStorage { get; set; }

        [Display(Name = "作为入库仓库")]
        public bool IsForAddInventory { get; set; }

        [Display(Name = "统计信息公开")]
        public virtual bool IsPublicInfo { get; set; }

        [Display(Name = "仓库简介")]
        public virtual string Description { get; set; }
        /// <summary>
        /// 盘点锁定
        /// </summary>
        [Display(Name="盘点锁定")]
        public virtual bool CheckLock { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }

        public virtual ICollection<Checker> checkers { get; set; } 

        public virtual ICollection<SaleAutoGen> SaleAutoGens { get; set; }
    }
}


