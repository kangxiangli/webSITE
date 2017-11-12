
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkOrderDealtWith : EntityBase<int>
    {
        /// <summary>
        /// 工单ID
        /// </summary>
        [Display(Name = "工单ID")]
        public virtual int WorkOrderId { get; set; }

        /// <summary>
        /// 处理人员ID
        /// </summary>
        [Display(Name = "处理人员ID")]
        public virtual int HandlerID { get; set; }

        /// <summary>
        /// 处理状态（-1：已拒绝；0：待处理：1：已接受；2：已完成）
        /// </summary>
        [Display(Name = "处理状态（-1：已拒绝；0：待处理：1：已接受；2：已完成）")]
        public virtual int Status { get; set; }

        /// <summary>
        /// 备注(若拒绝，则为拒绝原因)
        /// </summary>
        [Display(Name = "备注(若拒绝，则为拒绝原因)")]
        public virtual string Notes { get; set; }

        //[Display(Name = "是否已读")]
        //[DefaultValue(false)]
        //public virtual bool IsRead { get; set; }

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
        /// 处理人信息
        /// </summary>
        [Display(Name = "处理人信息")]
        [ForeignKey("HandlerID")]
        public virtual Administrator Handler { get; set; }

        /// <summary>
        /// 操作人信息
        /// </summary>
        [Display(Name = "操作人信息")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 工单信息
        /// </summary>
        [Display(Name = "工单信息")]
        [ForeignKey("WorkOrderId")]
        public virtual WorkOrder WorkOrder { get; set; }

    }
}

