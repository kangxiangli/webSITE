
//  <copyright file="TradeStatus.cs" company="优维拉软件设计工作室">



//  <last-date>2014-12-23 19:46</last-date>


// ReSharper disable InconsistentNaming
namespace Whiskey.Web.Net.Alipay
{
    /// <summary>
    /// 表示交易状态的枚举
    /// </summary>
    public enum TradeStatus
    {
        /// <summary>
        /// 等待买家付款
        /// </summary>
        WAIT_BUYER_PAY,

        /// <summary>
        /// 买家已付款，等待卖家发货
        /// </summary>
        WAIT_SELLER_SEND_GOODS,

        /// <summary>
        /// 卖家已发货，等待买家确认
        /// </summary>
        WAIT_BUYER_CONFIRM_GOODS,

        /// <summary>
        /// 交易成功结束
        /// </summary>
        TRADE_FINISHED,

        /// <summary>
        /// 交易中途关闭（已结束，未成功完成）
        /// </summary>
        TRADE_CLOSED
    }
}