
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class WorkOrderDealtWithDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "工单ID")]
        public virtual int WorkOrderId { get; set; }

        [Display(Name = "处理人员ID")]
        public virtual int HandlerID { get; set; }

        [Display(Name = "处理状态（-1：已拒绝；0：待处理：1：已接受；2：已完成）")]
        public virtual int Status { get; set; }

        [Display(Name = "备注(若拒绝，则为拒绝原因)")]
        public virtual string Notes { get; set; }

        [Display(Name = "更新时间")]
        public virtual DateTime UpdatedTime { get; set; }

        [Display(Name = "处理时间")]
        public virtual DateTime? DealtTime { get; set; }

        [Display(Name = "完成时间")]
        public virtual DateTime? FinishTime { get; set; }

        //[Display(Name = "是否已读")]
        //[DefaultValue(false)]
        //public virtual bool IsRead { get; set; }
    }
}


