
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class AppointmentGen : EntityBase<int>
    {
        public AppointmentGen()
        {
            Members = new List<Member>();
            Products = new List<Product>();
        }

        [Display(Name = "会员")]
        public virtual ICollection<Member> Members { get; set; }

        [Display(Name = "商品")]
        public virtual ICollection<Product> Products { get; set; }

        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndTime { get; set; }

        [Display(Name = "生成数")]
        public virtual int AllCount { get; set; }

        [Display(Name = "成功数")]
        public virtual int SuccessCount { get; set; }

        [Display(Name = "备注")]
        [StringLength(300)]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
		public virtual Administrator Operator { get; set; }
	}
}

