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
    public class Factorys : EntityBase<int>
    {
        public Factorys()
        {
            Designers = new List<Designer>();
        }

        [Display(Name = "工厂名称")]
        [StringLength(100)]
        public virtual string FactoryName { get; set; }

        [Display(Name = "工厂地址")]
        [StringLength(100)]
        public virtual string FactoryAddress { get; set; }

        [Display(Name = "负责人")]
        [StringLength(20)]
        public virtual string Leader { get; set; }

        [Display(Name = "联系电话")]
        [StringLength(20)]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "Mac地址")]
        [StringLength(100)]
        public virtual string MacAddress { get; set; }

        [Display(Name = "备注")]
        [StringLength(1000)]
        public virtual string Notes { get; set; }
        /// <summary>
        /// 工厂下的所有设计师
        /// </summary>
        public virtual ICollection<Designer> Designers { get; set; }

        [Display(Name = "所属部门")]
        public virtual int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [Display(Name = "所属店铺")]
        public virtual int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [Display(Name = "所属仓库")]
        public virtual int StorageId { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage Storage { get; set; }

        [Display(Name = "代理品牌")]
        public virtual int BrandId { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        public virtual ICollection<JobPosition> JobPositions { get; set; }
    }
}


