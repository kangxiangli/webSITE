
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AppointmentGenDto : IAddDto, IEditDto<int>
    {
        public AppointmentGenDto()
        {
            MemberIds = new List<int>();
            ProductIds = new List<int>();
        }
        public int Id { get; set; }

        [Display(Name = "会员")]
        [Required]
        public virtual ICollection<int> MemberIds { get; set; }

        [Display(Name = "商品")]
        [Required]
        public virtual ICollection<int> ProductIds { get; set; }

        [Display(Name = "开始时间")]
        [Required]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        [Required]
        public virtual DateTime EndTime { get; set; }

        [Display(Name = "备注")]
        [StringLength(300)]
        public virtual string Notes { get; set; }
    }
    /// <summary>
    /// 预约批量导入Dto
    /// </summary>
    public class AppointmentGenBatchDto
    {
        [Display(Name = "会员手机号")]
        [Required]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "商品货号")]
        [Required]
        public virtual string ProductNumber { get; set; }

        [Display(Name = "预约时间")]
        [Required]
        public virtual DateTime AppointmentTime { get; set; }
    }
}


