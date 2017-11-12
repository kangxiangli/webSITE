using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class CouponItemDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "优惠券")]
        public virtual int CouponId { get; set; }

        [Display(Name = "优惠卷编码")]
        [StringLength(13)]
        public virtual string CouponNumber { get; set; }

        [Display(Name = "发放会员")]
        public virtual int? MemberId { get; set; }

        [Display(Name = "是否使用")]
        public virtual bool IsUsed { get; set; }

        [Display(Name = "使用时间")]
        public virtual DateTime? UsedTime { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
