using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberSignDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "签到时间")]
        public virtual DateTime SignTime { get; set; }

        [Display(Name = "奖品类型")]
        public virtual int PrizeType { get; set; }

        [Display(Name = "积分")]
        public virtual int Score { get; set; }

        [Display(Name = "优惠券")]
        public virtual int? CouponId { get; set; }

        [Display(Name = "奖品")]
        public virtual int? PrizeId { get; set; }

        [Display(Name = "是否领取")]
        public virtual bool IsReceive { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
