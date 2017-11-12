using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class Rest : EntityBase<int>
    {
        public Rest()
        {
            AnnualLeaveDays = 0;
            PaidLeaveDays = 0;
            ChangeRestDays = 0;
        }

        [DisplayName("员工Id")]
        public virtual int? AdminId { get; set; }

        [DisplayName("年假天数")]
        public virtual double AnnualLeaveDays { get; set; }

        [DisplayName("带薪休假天数")]
        public virtual double PaidLeaveDays { get; set; }

        [DisplayName("调休天数")]
        public virtual double ChangeRestDays { get; set; }

        [DisplayName("补班天数")]
        public virtual double SupplementaryClassDays { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string Notes { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
