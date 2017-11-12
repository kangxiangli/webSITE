
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class OrderFood : EntityBase<int>
	{
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [Display(Name = "短信已发送")]
        public virtual bool smsIsSend { get; set; }

        [Display(Name = "预约员工")]
        public virtual ICollection<Administrator> Admins { get; set; }
    }
}

