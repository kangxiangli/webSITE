using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class RestDto : IAddDto, IEditDto<int>
    {        

        [Required(ErrorMessage="必选选择会员")]
        public virtual string RealName { get; set; }

        [DisplayName("员工Id")]
        public virtual int AdminId { get; set; }

        [DisplayName("年假天数")]
        public virtual double AnnualLeaveDays { get; set; }

        [DisplayName("带薪休假天数")]
        public virtual double PaidLeaveDays { get; set; }

        [DisplayName("调休天数")]
        public virtual double ChangeRestDays { get; set; }

        [DisplayName("补班天数")]
        public virtual double SupplementaryClassDays { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{0}")]
        public virtual string Notes { get; set; }

        public virtual Int32 Id { get; set; }
    }
}
