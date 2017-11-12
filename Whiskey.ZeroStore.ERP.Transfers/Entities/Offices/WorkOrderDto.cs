
using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class WorkOrderDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
        
        [Display(Name = "工单标题")]
        [StringLength(70, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string WorkOrderTitle { get; set; }

        [Display(Name = "工单类别ID")]
        public virtual int WorkOrderCategoryId { get; set; }

        [Display(Name = "申请人ID")]
        public virtual int ApplicantId { get; set; }

        [Display(Name = "部门ID")]
        public virtual int? DepartmentId { get; set; }

        [Display(Name = "申请内容")]
        public virtual string Content { get; set; }

        [Display(Name = "图片路径")]
        public virtual string ImgAddress { get; set; }

        [Display(Name = "处理状态（-1：已撤销；0：指派中；1：已分配；2：已接受；3：已完成）")]
        public virtual int Status { get; set; }

        [Display(Name = "经手人次")]
        public virtual int PersonHandleCount { get; set; }

        [Display(Name = "当前处理人员ID")]
        public virtual int? HandlerID { get; set; }

        [Display(Name = "处理时间")]
        public virtual DateTime? DealtTime { get; set; }

        [Display(Name = "完成时间")]
        public virtual DateTime? FinishTime { get; set; }

        [Display(Name = "更新时间")]
        public virtual DateTime? UpdatedTime { get; set; }
    }
}


