
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 物品采购表
    /// </summary>
    public class GoodsPurchasing : EntityBase<int>
    {
        /// <summary>
        /// 物品类别ID
        /// </summary>
        [Display(Name = "物品类别ID")]
        public virtual int CompanyGoodsCategoryID { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        [Display(Name = "总数量")]
        public virtual int Quantity { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        [Display(Name = "总金额")]
        public virtual decimal TotalAmount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Display(Name = "备注")]
        public virtual string Notes { get; set; }

        /// <summary>
        /// 经办人ID
        /// </summary>
        [Display(Name = "经办人ID")]
        //[ServiceStack.DataAnnotations.Default(0)]
        public virtual int AdminId { get; set; }

        /// <summary>
        /// 操作人员
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 经办人员
        /// </summary>
        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        /// <summary>
        /// 物品类别
        /// </summary>
        [ForeignKey("CompanyGoodsCategoryID")]
        public virtual CompanyGoodsCategory CompanyGoodsCategory { get; set; }
    }
}

