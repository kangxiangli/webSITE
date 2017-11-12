using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 优惠券详情
    /// </summary>
    [Serializable]
    public class CouponItem : EntityBase<int>
    {
        public CouponItem()
        {
            IsUsed = false;
        }
        [Display(Name = "优惠券")]
        public virtual int CouponId { get; set; }

        [Display(Name = "优惠卷编码")]
        [StringLength(13)]
        public virtual string CouponNumber { get; set; }

        [Display(Name = "接受会员")]
        public virtual int? MemberId { get; set; }

        [Display(Name = "合作商")]
        public virtual int? PartnerId { get; set; }

        [Display(Name = "是否使用")]
        public virtual bool IsUsed { get; set; }

        [Display(Name = "使用时间")]
        public virtual DateTime? UsedTime { get; set; }

        //[Display(Name = "条形码")]
        //[StringLength(150)]
        //public virtual string BarcodePath { get; set; }

        [ForeignKey("CouponId")]
        public virtual Coupon Coupon { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("PartnerId")]
        public virtual Partner Partner { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
