using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Checker : EntityBase<int>
    {
        public Checker()
        {
            CheckerItems = new List<CheckerItem>();
        }
        [Display(Name = "盘点标识符")]
        [StringLength(20)]
        public virtual string CheckGuid { get; set; }

        [Display(Name = "盘点店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "所属仓库")]
        public virtual int StorageId { get; set; }

        [Display(Name = "配货")]
        [Description("配货的标识,为null时表示对店铺的盘点，不为null时是针对配货的盘点")]
        public virtual int? OrberblankId { get; set; }

        [Display(Name = "品牌")]
        [Description("商品的品牌")]
        public virtual int? BrandId { get; set; }

        [Display(Name = "分类")]
        [Description("外套，裤子，大衣等分类")]
        public virtual int? CategoryId { get; set; }

        [Display(Name = "盘点名称")]
        [StringLength(30)]
        public virtual string CheckerName { get; set; }

        [Display(Name = "盘点前数量")]
        [Description("记录盘点前，库存的总数量")]
        public virtual int BeforeCheckQuantity { get; set; }

        [Display(Name = "盘点后数量")]
        [Description("记录盘点后，库存的总数量")]
        public virtual int AfterCheckQuantity { get; set; }

        [Display(Name = "已盘数量")]
        public virtual int CheckedQuantity { get; set; }

        [Display(Name = "有效数量")]
        public virtual int ValidQuantity { get; set; }

        [Display(Name = "缺货数量")]
        public virtual int MissingQuantity { get; set; }

        [Display(Name = "余货数量")]
        public virtual int ResidueQuantity { get; set; }

        [Display(Name = "盘点状态")]
        [Description("参照CheckerFlag")]
        public virtual CheckerFlag CheckerState { get; set; }

        [Display(Name = "盘点备注")]
        [StringLength(200)]
        public virtual string Notes { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("StorageId")]
        public virtual Storage Storage { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<CheckerItem> CheckerItems { get; set; }



    }

    /// <summary>
    /// 仓库盘点类型
    /// </summary>
    public enum CheckerFlag
    {

        /// <summary>
        /// 失败 0
        /// </summary>

        Fail = 0,

        /// <summary>
        /// 盘点中 1
        /// </summary>
        Checking = 1,

        /// <summary>
        /// 中断 2
        /// </summary>
        Interrupt = 2,

        /// <summary>
        /// 盘点完成 3
        /// </summary>
        Checked = 3,

        /// <summary>
        /// 完成校对 4
        /// </summary>
        Proofreader = 4,

        /// <summary>
        /// 其它 -1
        /// </summary>
        Other = -1
    }
}


