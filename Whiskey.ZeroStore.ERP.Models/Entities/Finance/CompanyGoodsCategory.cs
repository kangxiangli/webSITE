
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 公司物品类别表
    /// </summary>
	public class CompanyGoodsCategory : EntityBase<int>
    {
        public CompanyGoodsCategory()
        {
            Children = new List<CompanyGoodsCategory>();
        }


        /// <summary>
        /// 手动添加的唯一标识（只有唯一性的物品存在，非唯一性的可为空）
        /// </summary>
		[Display(Name = "唯一标识")]
        public virtual string UniqueIdentification { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        [Display(Name = "名称")]
        public virtual string CompanyGoodsCategoryName { get; set; }

        /// <summary>
        /// 类别所属类型  在CompanyGoodsCategoryTypeFlag里选择
        /// </summary>
        [Display(Name = "类型")]
        public virtual int Type { get; set; }

        /// <summary>
        /// 所属父类别ID
        /// </summary>
        [Display(Name = "父类别")]
        public virtual int? ParentId { get; set; }

        /// <summary>
        /// 是否唯一性
        /// </summary>
        [Display(Name = "唯一性")]
        public virtual bool IsUniqueness { get; set; }

        /// <summary>
        /// 图片路径(图片长宽不可超过500*500，图片大小不可超过300KB)
        /// </summary>
        [Display(Name = "展示图片")]
        public virtual string ImgAddress { get; set; }

        /// <summary>
        /// 物品总数量
        /// </summary>
        [Display(Name = "总数量")]
        public virtual int? TotalQuantity { get; set; }

        /// <summary>
        /// 已使用数量
        /// </summary>
        [Display(Name = "已使用数量")]
        public virtual int? UsedQuantity { get; set; }

        /// <summary>
        /// 物品价格(父类无价格)
        /// </summary>
        [Display(Name = "物品价格")]
        public virtual decimal? Price { get; set; }

        /// <summary>
        /// 状态（0：空闲；1：使用中；2：已损坏；3：维修中，参考CompanyGoodsCategoryStatusFlag）
        /// </summary>
        [Display(Name = "状态")]
        public virtual int Status { get; set; }

        /// <summary>
        /// 类别备注
        /// </summary>
        [Display(Name = "备注")]
        public virtual string Notes { get; set; }

        /// <summary>
        /// 操作人信息
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 父类别信息
        /// </summary>
        [ForeignKey("ParentId")]
        public virtual CompanyGoodsCategory Parent { get; set; }

        [Description("子集")]
        public virtual ICollection<CompanyGoodsCategory> Children { get; set; }
    }
}

