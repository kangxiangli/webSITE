
using System;
using Whiskey.Core.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OrderFoodDto : IAddDto, IEditDto<int>
    {
        public OrderFoodDto()
        {
            AdminIds = new List<int>();
        }
        public int Id { get; set; }

        [Display(Name = "短信已发送")]
        public virtual bool smsIsSend { get; set; }

        [Display(Name = "预约员工")]
        [Required]
        public virtual ICollection<int> AdminIds { get; set; }
    }
}


