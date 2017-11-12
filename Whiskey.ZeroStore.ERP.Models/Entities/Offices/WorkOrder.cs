
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class WorkOrder : EntityBase<int>
	{
        /// <summary>
        /// 工单标题
        /// </summary>
		[Display(Name = "工单标题")]
        [StringLength(70, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string WorkOrderTitle { get; set; }

        /// <summary>
        /// 工单类别ID
        /// </summary>
        [Display(Name = "工单类别ID")]
        public virtual int WorkOrderCategoryId { get; set; }

        /// <summary>
        /// 申请人ID
        /// </summary>
        [Display(Name = "申请人ID")]
        public virtual int ApplicantId { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [Display(Name = "部门ID")]
        public virtual int? DepartmentId { get; set; }

        /// <summary>
        /// 工单内容
        /// </summary>
        [Display(Name = "工单内容")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        [Display(Name = "图片路径")]
        public virtual string ImgAddress { get; set; }

        /// <summary>
        /// 处理状态（-1：已撤销；0：指派中；1：已分配；2：已接受；3：已完成）
        /// </summary>
        [Display(Name = "处理状态（-1：已撤销；0：指派中；1：已分配；2：已接受；3：已完成）")]
        public virtual int Status { get; set; }

        /// <summary>
        /// 经手人次
        /// </summary>
        [Display(Name = "经手人次")]
        public virtual int PersonHandleCount { get; set; }

        /// <summary>
        /// 当前处理人员ID
        /// </summary>
        [Display(Name = "当前处理人员ID")]
        public virtual int? HandlerID { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        [Display(Name = "处理时间")]
        public virtual DateTime? DealtTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Display(Name = "完成时间")]
        public virtual DateTime? FinishTime { get; set; }

        /// <summary>
        /// 工单类别信息
        /// </summary>
        [Display(Name = "工单类别信息")]
        [ForeignKey("WorkOrderCategoryId")]
        public virtual WorkOrderCategory WorkOrderCategory { get; set; }

        /// <summary>
        /// 申请人信息
        /// </summary>
        [Display(Name = "申请人信息")]
        [ForeignKey("ApplicantId")]
        public virtual Administrator Applicant { get; set; }

        /// <summary>
        /// 部门信息
        /// </summary>
        [Display(Name = "部门信息")]
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        /// <summary>
        /// 当前处理人信息
        /// </summary>
        [Display(Name = "当前处理人信息")]
        [ForeignKey("HandlerID")]
        public virtual Administrator Handler { get; set; }

        /// <summary>
        /// 经手过该工作的所有工单处理数据
        /// </summary>
        [Display(Name = "经手过该工作的所有工单处理数据")]
        [ForeignKey("Id")]
        public virtual WorkOrderDealtWith[] WorkOrderDealtWiths { get; set; }

        /// <summary>
        /// 操作人员
        /// </summary>
        [Display(Name = "操作人员")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

