using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 退货单实体,退货时,每次退货的各项金额存储在退货单中,
    /// 如果是分多次部分退货,每次都要查询此记录,计算各项可退的金额
    /// </summary>
    [Serializable]
    public class Returned : EntityBase<int>
    {
        public Returned()
        {
            ReturnedItems = new List<ReturnedItem>();
        }

        /// <summary>
        /// 所属店铺
        /// </summary>
        [Display(Name = "所属店铺")]
        public virtual int? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        /// <summary>
        /// 购买会员
        /// </summary>
        [Display(Name = "购买会员")]
        public virtual int? MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        /// <summary>
        /// 退货单号
        /// </summary>
        [Display(Name = "退货单号")]
        [StringLength(50)]
        public virtual string ReturnedNumber { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        [Display(Name = "订单编号")]
        [StringLength(50)]
        public virtual string RetailNumber { get; set; }


        /// <summary> 
        /// 上级换货单编号（如果当前换货单是由另一张换货单生成的，会保存下之前换货单的编号）
        /// </summary>
        [Display(Name = "上级换货单编号")]
        [StringLength(50)][Obsolete("已废弃",true)]
        public string ParentExchangeNumber { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public int? RetailId { get; set; }

        [ForeignKey("RetailId")]
        public virtual Retail Retail { get; set; }

        #region 扣除项

        /// <summary>
        /// 抹去扣除
        /// </summary>
        [Display(Name = "抹去扣除")]
        public decimal EraseMoney { get; set; }

        /// <summary>
        /// 优惠券扣除
        /// </summary>
        [Display(Name = "优惠券扣除")]
        public decimal Coupon { get; set; }

        /// <summary>
        /// 店铺活动扣除
        /// </summary>
        [Display(Name = "店铺活动扣除")]
        public decimal StoreActivityDiscount { get; set; }

        /// <summary>
        /// 退货原因
        /// </summary>
        [StringLength(200)]
        public string Reason { get; set; }
        #endregion 扣除项

        #region 退还项

        /// <summary>
        /// 现金退还
        /// </summary>
        [Display(Name = "现金退还")]
        public decimal Cash { get; set; }

        /// <summary>
        /// 刷卡退还
        /// </summary>
        [Display(Name = "刷卡退还")]
        public decimal SwipCard { get; set; }

        public SwipeCardType? SwipeCardType { get; set; }

        /// <summary>
        /// 消费积分退还
        /// </summary>
        [Display(Name = "消费积分退还")]
        public decimal ConsumeScore { get; set; }

        /// <summary>
        /// 获得积分退还
        /// </summary>
        [Display(Name = "获得积分退还")]
        public decimal AchieveScore { get; set; }

        /// <summary>
        /// 储值退还
        /// </summary>
        [Display(Name = "储值退还")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 计算储值成本的实际值
        /// </summary>
        public decimal RealBalance { get; set; }

        /// <summary>
        /// 真实返还总额(现金+刷卡+实际储值)
        /// </summary>
        [NotMapped]
        public decimal RealReturnMoney
        {
            get
            {
                var value = Cash + SwipCard + RealBalance;
                return value <= 0 ? 0 : value;
            }
        }


        #endregion 退还项


        [Display(Name = "退货类型")]
        public virtual ReturnType Status { get; set; }

        /// <summary>
        /// 退货明细项
        /// </summary>

        public virtual ICollection<ReturnedItem> ReturnedItems { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }

}