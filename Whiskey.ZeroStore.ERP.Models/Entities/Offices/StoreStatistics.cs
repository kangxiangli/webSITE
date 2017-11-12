using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Notices
{
    public class StoreStatistics : EntityBase<int>
    {
        public StoreStatistics()
        {
            StatDate = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
        }
        /// <summary>
        /// 统计店铺id
        /// </summary>
        [Display(Name = "统计店铺id")]
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        /// <summary>
        /// 成本(销售商品的吊牌价总和)
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// 盈利=实际销售总额-实际退货总额-成本(未退货商品) => (现金+刷卡+储值成本) - 销售商品的吊牌价总和
        /// </summary>
        public decimal Gain { get; set; }

        #region 销售总计
        /// <summary>
        /// 销售商品数量
        /// </summary>
        [Display(Name = "销售总量")]
        public int RetailCount { get; set; }


        /// <summary>
        /// 一天内的服装零售总额，每一单的销售总额累计，不考虑储值成本
        /// </summary>
        [Display(Name = "销售总额")]
        public decimal RetailAmount { get; set; }



        [Display(Name = "销售单数统计")]
        public int RetailOrderCount { get; set; }


        /// <summary>
        /// 实际销售总额=现金+刷卡+储值成本，不考虑积分消费
        /// </summary>
        [Display(Name = "实际销售总额")]
        public decimal RealRetailAmount { get; set; }

        #endregion

        #region 销售明细统计
        /// <summary>
        /// 现金消费
        /// </summary>
        [Display(Name = "现金消费")]
        public decimal CashConsume { get; set; }

        /// <summary>
        /// 刷卡总消费
        /// </summary>
        [Display(Name = "刷卡总消费")]
        public decimal SwipCardConsume { get; set; }


        /// <summary>
        /// 积分总消费
        /// </summary>
        [Display(Name = "积分总消费")]
        public decimal ScoreConsume { get; set; }


        /// <summary>
        /// 储值总消费
        /// </summary>
        [Display(Name = "储值总消费")]
        public decimal BalanceConsume { get; set; }

        /// <summary>
        /// 实际储值总消费成本
        /// </summary>
        [Display(Name = "实际储值总消费")]
        public decimal RealBalanceConsume { get; set; }


        /// <summary>
        /// 总共抹除
        /// </summary>
        [Display(Name = "总共抹除")]
        public decimal Erase { get; set; }

        /// <summary>
        /// 总共找零
        /// </summary>
        [Display(Name = "总共找零")]
        public decimal ReturnMoney { get; set; }
        #endregion

        #region 优惠券使用,商品活动统计
        /// <summary>
        /// 使用优惠券次数
        /// </summary>
        [Display(Name = "使用优惠券次数")]
        public int CouponConsumeCount { get; set; }


        /// <summary>
        /// 优惠券总共抵扣额度
        /// </summary>
        [Display(Name = "优惠券总优惠金额")]
        public decimal CouponConsumeMoney { get; set; }


        /// <summary>
        /// 参与商品活动次数
        /// </summary>
        [Display(Name = "参与商品活动次数")]
        public int SaleCampaignConsumeCount { get; set; }

        /// <summary>
        /// 参与店铺活动次数
        /// </summary>
        [Display(Name = "参与店铺活动次数")]
        public int StoreActivityConsumeCount { get; set; }
        #endregion

        #region 退货统计
        /// <summary>
        /// 退货总件数
        /// </summary>
        [Display(Name = "退货总件数")]
        public int ReturnedCount { get; set; }

        /// <summary>
        /// 退货单数
        /// </summary>
        [Display(Name = "退货单数")]
        public int ReturnedOrderCount { get; set; }

        /// <summary>
        /// 退货金额累计(现金+刷卡+储值)，不考虑储值成本
        /// </summary>
        [Display(Name = "退货总金额")]
        public decimal ReturnedAmount { get; set; }


        /// <summary>
        /// 考虑储值成本(现金+刷卡+储值成本)
        /// </summary>
        [Display(Name = "真实退货总额")]
        public decimal RealReturnedAmount { get; set; }

        /// <summary>
        /// 返还现金
        /// </summary>
        [Display(Name = "返还现金")]
        public decimal ReturnedCash { get; set; }

        /// <summary>
        /// 返还刷卡
        /// </summary>

        [Display(Name = "返还刷卡")]
        public decimal ReturnedSwipCard { get; set; }

        /// <summary>
        /// 返还储值
        /// </summary>
        [Display(Name = "返还储值")]
        public decimal ReturnedBalance { get; set; }
        /// <summary>
        /// 返还储值实际成本
        /// </summary>
        [Display(Name = "返还储值实际成本")]
        public decimal RealReturnedBalance { get; set; }

        /// <summary>
        /// 返还积分
        /// </summary>
        [Display(Name = "返还积分")]
        public decimal ReturnedScore { get; set; }
        #endregion

        #region 会员统计
        /// <summary>
        /// 新增会员数
        /// </summary>
        public int AddMemberCount { get; set; }

        /// <summary>
        /// 会员消费人数(订单中会员人数)
        /// </summary>
        [Display(Name = "会员消费人数")]
        public int MemberCountFromOrder { get; set; }

        /// <summary>
        /// 非会员消费人数(订单中非会员人数)
        /// </summary>
        [Display(Name = "非会员消费人数")]
        public int NoMemberCountFromOrder { get; set; }


        /// <summary>
        /// 充值储值的次数
        /// </summary>
        [Display(Name = "充储值次数")]
        public int MemberCountFromRechargeBalance { get; set; }


        /// <summary>
        /// 充储值总额,不含赠送
        /// </summary>

        [Display(Name = "充储值总额")]

        public decimal MemberRechargeBalanceAmount { get; set; }

        /// <summary>
        /// 充值积分次数
        /// </summary>
        [Display(Name = "充积分次数")]
        public int MemberCountFromRechargeScore { get; set; }

        [Display(Name = "充积分总额")]
        public decimal MemberRechargeScoreAmount { get; set; }
        #endregion

        #region 员工统计
        /// <summary>
        /// 有销售业绩的员工数量
        /// </summary>
        [Display(Name = "有销售业绩的员工数量")]
        public int EmployeeCountFromOrder { get; set; }

        /// <summary>
        /// 有销售业绩的员工AdminId
        /// </summary>
        [StringLength(500)]
        public string EmployeeIdsFromOrder { get; set; }
        #endregion


        #region 换货统计

        /// <summary>
        /// 换出数量(原订单商品)
        /// </summary>
        public int ExchangeOrderOriginProductQuantity { get; set; }

        /// <summary>
        /// 换入数量(新商品)
        /// </summary>
        public int ExchangeOrderNewProductQuantity { get; set; }

        /// <summary>
        /// 换货补差价总额
        /// </summary>
        public decimal ExchangeOrderPayAmount { get; set; }

        /// <summary>
        /// 换货退还储值总额
        /// </summary>
        public decimal ExchangeOrderReturnBalanceAmount { get; set; }

        /// <summary>
        /// 实际换货退还储值成本
        /// </summary>
        public decimal ExchangeOrderRealReturnBalanceAmount { get; set; }

        #endregion

        /// <summary>
        /// 配货单发货数量
        /// </summary>
        public int OrderblankDeliverCount { get; set; }

        /// <summary>
        /// 配货单收货数量
        /// </summary>
        public int OrderblankAcceptCount { get; set; }

        /// <summary>
        /// 可用库存数量
        /// </summary>
        public int? InventoryCount { get; set; }

        /// <summary>
        /// 入库数量
        /// </summary>
        public int InventoryAddCount { get; set; }

        /// <summary>
        /// 统计日期,存储格式:20161228
        /// </summary>
        public int StatDate { get; set; }
    }
}
