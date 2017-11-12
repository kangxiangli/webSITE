
//  <copyright file="LogisticsType.cs" company="优维拉软件设计工作室">



//  <last-date>2014-12-23 19:48</last-date>


// ReSharper disable InconsistentNaming
namespace Whiskey.Web.Net.Alipay
{
    /// <summary>
    /// 表示物流类型的枚举
    /// </summary>
    public enum LogisticsType
    {
        /// <summary>
        /// 平邮
        /// </summary>
        POST = 0,

        /// <summary>
        /// 快递
        /// </summary>
        EXPRESS = 1,

        /// <summary>
        /// EMS
        /// </summary>
        EMS = 2,

        /// <summary>
        /// 无需物流，在发货时使用
        /// </summary>
        DIRECT = 3
    }
}