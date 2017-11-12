using System;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 前台页面提交过来的退款信息
    /// </summary>
    public class ReturnInfoModel
    {
        public ReturnDetail ReturnDetail { get; set; }
        public List<string> ProductBarcodes { get; set; }

        public string Note { get; set; }
        public DateTime Returntime { get; set; }
        public string RetailNumber { get; set; }
    }

    /// <summary>
    /// 计算得到的退款信息各项金额
    /// </summary>
    public class ReturnDetail
    {
        /// <summary>
        /// 退款总额
        /// </summary>
        public decimal ReturnMoneyCou { get; set; }

        /// <summary>
        /// 退现金
        /// </summary>
        public decimal ReturnCash { get; set; }

        /// <summary>
        /// 退积分
        /// </summary>
        public decimal ReturnScore { get; set; }

        /// <summary>
        /// 退储值
        /// </summary>
        public decimal ReturnCardValue { get; set; }

        /// <summary>
        /// 退刷卡
        /// </summary>
        public decimal ReturnSwipCard { get; set; }

        /// <summary>
        /// 刷卡类型
        /// </summary>
        public SwipeCardType? SwipeCardType { get; set; }

        /// <summary>
        /// 扣除获得积分
        /// </summary>
        public decimal ReturnGetScore { get; set; }

        /// <summary>
        /// 扣除抹零
        /// </summary>
        public decimal EraseMoney { get; set; }

        /// <summary>
        /// 扣除优惠券优惠
        /// </summary>
        public decimal Coupon { get; set; }

        /// <summary>
        /// 扣除店铺活动优惠
        /// </summary>
        public decimal StoreActivityDiscount { get; set; }

        /// <summary>
        /// 经办人
        /// </summary>
        public string Admin { get; set; }

        /// <summary>
        /// 退货数量
        /// </summary>
        public int? ReturnCount { get; set; }

    }
}