using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OnlineOrderDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "订单编号")]
        [StringLength(12)]
        public virtual string OrderNumber { get; set; }

        [Display(Name = "购买会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "订单来源")] //网站 iOS Android
        public virtual int OrderSource { get; set; }

        [Display(Name = "订单状态")]
        public virtual int OrderType { get; set; }

        [Display(Name = "收获地址")]
        public virtual int MemberAddressId { get; set; }

        [Display(Name = "总价")]
        public virtual decimal TotalPrice { get; set; }

        public Int32 Id { get; set; }
    }
}
