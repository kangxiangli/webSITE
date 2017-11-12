
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    /// <summary>
    /// 物品申领表
    /// </summary>
    public class ClaimForGoodsDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
        /// <summary>
        /// 申请人员ID
        /// </summary>
		[Display(Name = "申请人员ID")]
        public virtual int ApplicantId { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [Display(Name = "部门ID")]
        public virtual int DepartmentId { get; set; }

        /// <summary>
        /// 物品类别ID
        /// </summary>
        [Display(Name = "物品ID")]
        public virtual int CompanyGoodsCategoryID { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        public virtual int Quantity { get; set; }

        /// <summary>
        /// 是否需归还
        /// </summary>
        [Display(Name = "是否需归还")]
        public virtual bool IsReturn { get; set; }

        /// <summary>
        /// 归还时间限制(false：不限制；true：需准确时间)
        /// </summary>
        [Display(Name = "归还时间限制")]
        public virtual bool ReturnTimeLimit { get; set; }

        /// <summary>
        /// 归还时间
        /// </summary>
        [Display(Name = "归还时间")]
        public virtual DateTime? ReturnTime { get; set; }

        /// <summary>
        /// 预计归还时间
        /// </summary>
        [Display(Name = "预计归还时间")]
        public virtual DateTime? EstimateReturnTime { get; set; }

        /// <summary>
        /// 归还状态（1：暂未归还；2：已归还）
        /// </summary>
        [Display(Name = "归还状态（1：暂未归还；2：已归还）")]
        [ServiceStack.DataAnnotations.Default(1)]
        public virtual int ReturnStatus { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Display(Name = "备注")]
        public virtual string Notes { get; set; }

        /// <summary>
        /// 申请人员
        /// </summary>
        [ForeignKey("ApplicantId")]
        public virtual Administrator Applicant { get; set; }

        /// <summary>
        /// 申请人部门
        /// </summary>
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        /// <summary>
        /// 类别备注
        /// </summary>
        [Display(Name = "操作人Id")]
        public virtual int OperatorId { get; set; }

        /// <summary>
        /// 操作人员
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 物品
        /// </summary>
        [ForeignKey("CompanyGoodsCategoryID")]
        public virtual CompanyGoodsCategory CompanyGoodsCategory { get; set; }
    }
}


