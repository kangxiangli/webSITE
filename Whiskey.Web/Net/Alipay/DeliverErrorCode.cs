
//  <copyright file="DeliverErrorCode.cs" company="优维拉软件设计工作室">



//  <last-date>2014-12-23 20:59</last-date>


// ReSharper disable InconsistentNaming
namespace Whiskey.Web.Net.Alipay
{
    /// <summary>
    /// 确认发货业务错误码
    /// </summary>
    public enum DeliverErrorCode
    {
        /// <summary>
        /// 参数不正确
        /// </summary>
        ILLEGAL_ARGUMENT,

        /// <summary>
        /// 指定交易信息不存在
        /// </summary>
        TRADE_NOT_EXIST,

        /// <summary>
        /// 交易状态不正确
        /// </summary>
        TRADE_STATUS_NOT_AVAILD,

        /// <summary>
        /// 签名不正确
        /// </summary>
        ILLEGAL_SIGN
    }
}