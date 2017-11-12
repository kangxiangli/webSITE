using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 优惠券活动
    /// </summary>

    public class CouponActivity : EntityBase<int>
    {
        public CouponActivity()
        {
            Coupons = new List<LBSCouponEntity>();
        }


        [Display(Name = "活动名称")]
        [StringLength(20)]
        [Required(ErrorMessage = "0～20个字符")]
        public virtual string ActivityName { get; set; }



        [Display(Name = "唯一标识")]
        [StringLength(32)]
        [Index(IsClustered =false,IsUnique = true)]
        public string ActivityGUID { get; set; }


        [Display(Name = "活动开始时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime ActivityStartDate { get; set; }

        [Display(Name = "活动结束时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime ActivityEndDate { get; set; }



        [Display(Name = "活动优惠券开始生效时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime CouponStartDate { get; set; }

        [Display(Name = "活动优惠券开始到期时间")]
        [Required(ErrorMessage = "请填写有效时间")]
        public virtual DateTime CouponEndDate { get; set; }




        [Display(Name = "备注")]
        [StringLength(200, ErrorMessage = "最多不能超过{1}个字符")]
        public virtual string Notes { get; set; }


        [Display(Name = "优惠券类型")][Required]
        public CouponActivityTypeEnum? CouponType { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<LBSCouponEntity> Coupons { get; set; }
    }

    /// <summary>
    /// 基于地理位置的优惠券
    /// </summary>
    public class LBSCouponEntity : EntityBase<int>
    {
        [Display(Name = "活动id")]
        public virtual int CouponActivityId { get; set; }


        [Display(Name = "优惠卷编码")]
        [StringLength(32)]
        [Index(IsClustered = false, IsUnique = true)]
        public virtual string CouponNumber { get; set; }


        [Display(Name = "优惠卷名称")]
        [StringLength(20)]
        [Index(IsClustered = false, IsUnique = false)]
        public virtual string Name { get; set; }


        [Display(Name = "优惠券类型")]
        public CouponActivityTypeEnum? CouponType { get; set; }


        [Display(Name = "金额")]
        public decimal Amount { get; set; }


        [Display(Name = "会员id")]
        public virtual int? MemberId { get; set; }


        [Display(Name = "是否使用")]
        public virtual bool IsUsed { get; set; }

        [Display(Name = "使用时间")]
        public virtual DateTime? UsedTime { get; set; }

        [StringLength(20)]
        [Required]
        public string Longtitude { get; set; }

        [StringLength(20)]
        [Required]
        public string Latitude { get; set; }

       
        [ForeignKey("CouponActivityId")]
        public virtual CouponActivity CouponActivity { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }

    public class CouponActivityConfiguration : EntityConfigurationBase<CouponActivity, int>
    {
        public CouponActivityConfiguration()
        {
            ToTable("C_CouponActivity");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
    public class CouponEntityConfiguration : EntityConfigurationBase<LBSCouponEntity, int>
    {
        public CouponEntityConfiguration()
        {
            ToTable("C_LBSCouponEntity");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }

    /// <summary>
    /// 优惠券类型
    /// </summary>
    public enum CouponActivityTypeEnum
    {
        代金券 = 0,
        积分券 = 1
    }
}
