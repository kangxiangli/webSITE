using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    /// <summary>
    /// 商品零售
    /// </summary>
    public partial class Retail : EntityBase<int>
    {
        public Retail()
        {
            PaymentType = PaymentType.Offline;
            ReturnRecordHistory = new List<ReturnedItem>();
            RetailItems = new List<RetailItem>();
        }

        /// <summary>
        /// 商品零售编号
        /// </summary>
        [Display(Name = "零售编号")]
        [StringLength(50)]
        public virtual string RetailNumber { get; set; }

        [StringLength(50, ErrorMessage = "长度超过限制")]
        public string TradeCredential { get; set; }

        [StringLength(50, ErrorMessage = "长度超过限制")]
        public string TradeReferNumber { get; set; }

        /// <summary>
        /// 消费总额
        /// </summary>
        [Display(Name = "消费总额")]
        public decimal ConsumeCount { get; set; }


        /// <summary>
        /// 等级折扣优惠
        /// </summary>
        public decimal LevelDiscountAmount { get; set; }

        /// <summary>
        /// 等级折扣
        /// </summary>
        public decimal? LevelDiscount { get; set; }

        /// <summary>
        /// 扣掉等级折扣之后的总消费金额
        /// 用于换货,退货时总额的计算
        /// </summary>
        [NotMapped]
        public decimal TotalMoneyAfterLevelDiscount
        {
            get
            {
                if (this.LevelDiscount.HasValue && this.LevelDiscountAmount > 0)
                {
                    return Math.Round(this.ConsumeCount * this.LevelDiscount.Value, 2);
                }
                return this.ConsumeCount;
            }
        }

        [NotMapped]
        /// <summary>
        /// 真实消费总额(现金+刷卡+实际储值消费)
        /// </summary>
        public decimal RealConsumeMoney
        {
            get
            {
                var value = this.CashConsume + this.SwipeConsume + this.RealStoredValueConsume;
                return value <= 0 ? 0 : value;
            }
        }

        /// <summary>
        /// 现金消费
        /// </summary>
        [Display(Name = "现金消费")]
        public decimal CashConsume { get; set; }

        /// <summary>
        /// 刷卡消费
        /// </summary>
        [Display(Name = "刷卡消费")]
        public decimal SwipeConsume { get; set; }

        /// <summary>
        /// 储值消费
        /// </summary>
        [Display(Name = "储值消费")]
        public decimal StoredValueConsume { get; set; }

        /// <summary>
        /// 储值系数,用于会员订单退货时计算储值退还成本
        /// </summary>
        [Range(0, 1)]
        public decimal? Quotiety { get; set; }

        /// <summary>
        /// 储值成本
        /// </summary>
        public decimal RealStoredValueConsume { get; set; }

        /// <summary>
        /// 剩余储值
        /// </summary>
        [Display(Name = "剩余储值")]
        public decimal RemainValue { get; set; }

        /// <summary>
        /// 积分消费
        /// </summary>
        [Display(Name = "积分消费")]
        public decimal ScoreConsume { get; set; }

        /// <summary>
        /// 获得积分
        /// </summary>
        [Display(Name = "获得积分")]
        public decimal GetScore { get; set; }

        /// <summary>
        /// 剩余积分
        /// </summary>
        [Display(Name = "剩余积分")]
        public decimal RemainScore { get; set; }

        /// <summary>
        /// 抹去
        /// </summary>
        [Display(Name = "抹去")]
        public decimal EraseConsume { get; set; }

        /// <summary>
        /// 找零
        /// </summary>
        [Display(Name = "找零")]
        public decimal ReturnMoney { get; set; }

        /// <summary>
        /// 消费会员
        /// </summary>
        [Display(Name = "消费会员")]
        public virtual int? ConsumerId { get; set; }

        /// <summary>
        /// 搭配师编号
        /// </summary>
        [Display(Name = "搭配师编号")]
        [StringLength(50)]
        public virtual string CollocationNumber { get; set; }

        /// <summary>
        /// 出库时间
        /// </summary>
        [Display(Name = "出库时间")]
        public virtual DateTime OutStorageDatetime { get; set; }

        /// <summary>
        /// 售出店铺
        /// </summary>
        [Display(Name = "售出店铺")]
        public virtual int? StoreId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(20, ErrorMessage = "备注长度在20个字符以内")]
        public virtual string Note { get; set; }

        /// <summary>
        /// 优惠券编号
        /// </summary>
        [Display(Name = "优惠券编号")]
        [StringLength(20)]
        public virtual string CouponNumber { get; set; }

        /// <summary>
        /// 优惠券抵扣金额
        /// </summary>
        public decimal CouponConsume { get; set; }


        public int? CouponItemId { get; set; }

        [ForeignKey("CouponItemId")]
        public virtual CouponItem CouponItem { get; set; }

        /// <summary>
        /// 店铺活动id
        /// </summary>
        public int? StoreActivityId { get; set; }

        /// <summary>
        /// 店铺活动优惠金额
        /// </summary>
        public decimal StoreActivityDiscount { get; set; }

        /// <summary>
        /// 采用的积分规则
        /// </summary>
        public virtual int? ScoreRuleId { get; set; }

        /// <summary>
        /// 参照RetailStatus 状态 0:正常 1：整单退货 2：部分退货 3.删除 4.禁用 5.发货完成
        /// </summary>
        public virtual RetailStatus RetailState { get; set; }

        /// <summary>
        /// 支付种类（线下支付，支付宝，微信...）
        /// </summary>
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// 刷卡消费类型
        /// </summary>
        public SwipeCardType? SwipeCardType { get; set; }

        /// <summary>
        /// 线上支付金额
        /// </summary>
        public decimal OnlinePaymentAmount { get; set; }

        /// <summary>
        /// 推荐人
        /// </summary>
        public virtual int? RecommenderId { get; set; }

        /// <summary>
        /// 推荐人
        /// </summary>

        [ForeignKey("RecommenderId")]
        public virtual Member Recommender { get; set; }

        /// <summary>
        /// 是否已经被打分
        /// </summary>
        public bool IsRated { get; set; }

        /// <summary>
        /// 顾客打分，[0-5]
        /// </summary>
        [Range(0, 5, ErrorMessage = "评分范围在0-5之间")]
        public int RatePoints { get; set; }

        public virtual ICollection<RetailItem> RetailItems { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("ConsumerId")]
        public virtual Member Consumer { get; set; }

        [ForeignKey("ScoreRuleId")]
        public virtual ScoreRule ScoreRule { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        /// <summary>
        /// 退货记录明细
        /// </summary>
        public virtual ICollection<ReturnedItem> ReturnRecordHistory { get; set; }


        [ForeignKey("StoreActivityId")]
        public virtual StoreActivity StoreActivity { get; set; }
    }





}