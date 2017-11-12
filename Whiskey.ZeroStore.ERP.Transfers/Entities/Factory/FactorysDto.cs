using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class FactorysDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

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

        [Display(Name = "所属部门")]
        [Required]
        public virtual int DepartmentId { get; set; }

        [Display(Name = "所属店铺")]
        [Required]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属仓库")]
        [Required]
        public virtual int StorageId { get; set; }

        [Display(Name = "代理品牌")]
        [Required]
        public virtual int BrandId { get; set; }

    }
}
