using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 零售页会员登录通过后返回给页面的登录信息
    /// </summary>
    public class MemberLoginPassDTO
    {
        public MemberLoginPassDTO()
        {
            Coupon = new List<MemberCoupon>();
        }
        public int Id { get; set; }
        public string MembNum { get; set; }
        public string MemberName { get; set; }
        public string RealName { get; set; }
        public decimal Balance { get; set; }
        public decimal Score { get; set; }
        public MemberCollocator Collocation { get; set; }
        public List<MemberCoupon> Coupon { get; set; }
        public string uuid { get; set; }
        public int? StoreId { get; set; }
        public string UniquelyIdentifies { get; set; }
        public float? LevelDiscount { get; set; }
    }

    public class MemberCollocator
    {
        /// <summary>
        /// 搭配师姓名
        /// </summary>
        public string CollocationName { get; set; }

        /// <summary>
        /// 搭配师id
        /// </summary>
        public string CollocationNum { get; set; }
    }

    public class MemberCoupon
    {
        public int Id { get; set; }
        /// <summary>
        /// 优惠券号码
        /// </summary>
        public string CouponNumb { get; set; }

        /// <summary>
        /// 优惠券名称
        /// </summary>
        public string CouponName { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal DiscountAmount { get; set; }
    }
}
