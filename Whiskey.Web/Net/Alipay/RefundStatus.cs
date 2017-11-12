
//  <copyright file="RefundStatus.cs" company="优维拉软件设计工作室">



//  <last-date>2014-12-23 19:47</last-date>


// ReSharper disable InconsistentNaming
namespace Whiskey.Web.Net.Alipay
{
    /// <summary>
    /// 表示退款状态的枚举
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// 无退款
        /// </summary>
        NONE,

        /// <summary>
        /// 退款协议等待卖家确认
        /// </summary>
        WAIT_SELLER_AGREE,

        /// <summary>
        /// 卖家不同意协议，等待买家修改
        /// </summary>
        SELLER_REFUSE_BUYER,

        /// <summary>
        /// 退款协议达成，等待买家退货
        /// </summary>
        WAIT_BUYER_RETURN_GOODS,

        /// <summary>
        /// 等待卖家收货
        /// </summary>
        WAIT_SELLER_CONFIRM_GOODS,

        /// <summary>
        /// 退款成功
        /// </summary>
        REFUND_SUCCESS,

        /// <summary>
        /// 退款关闭
        /// </summary>
        REFUND_CLOSED
    }
}