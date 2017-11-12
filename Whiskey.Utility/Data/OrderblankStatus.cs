using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Whiskey.Utility.Data
{
    public enum OrderblankStatus
    {
        //0:未配货 1：已配货 2：拒绝配货 3：被废除 4:收货
        /// <summary>
        /// 待配货
        /// </summary>
        [Description("待配货")]
        UnDelivery,
        /// <summary>
        /// 已配货
        /// </summary>
        [Description("已配货")]
        Deliveries,
        /// <summary>
        /// 拒绝配货
        /// </summary>
        [Description("拒绝配货")]
        NoDelivery,
        /// <summary>
        /// 配货单被废除
        /// </summary>
        [Description("配货单被废除")]
        Destory,
        /// <summary>
        /// 收货
        /// </summary>
        [Description("收货")]
        TakeDelivery

    }
}
