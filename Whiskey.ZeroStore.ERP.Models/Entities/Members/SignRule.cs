using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 签到有奖信息
    /// </summary>
    public class SignRule : EntityBase<int>
    {

        [Display(Name = "规则名称")]
        [Required(ErrorMessage="不能为空")]
        [StringLength(20,ErrorMessage="最大长度超过{1}")]
        public virtual string SignRuleName { get; set; }

        [Display(Name = "第x天")]
        [Index]
        public virtual int Week { get; set; }

        [Display(Name = "奖品类型")]
        public virtual int PrizeType { get; set; }

        [Display(Name = "优惠券")]
        public virtual int? CouponId { get; set; }

        [Display(Name = "奖品")]
        public virtual int? PrizeId { get; set; }

        [Display(Name = "备注")]
        [StringLength(120, ErrorMessage = "最大长度超过{1}")]
        public virtual string Notes { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; }

        [ForeignKey("PrizeId")]
        public virtual Prize Prize { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
