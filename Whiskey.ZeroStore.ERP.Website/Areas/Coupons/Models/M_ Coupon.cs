using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Coupons.Models
{
    public class M__Coupon
    {
        /// <summary>
        /// 供应商
        /// </summary>
        public int PartnerId { get; set; }

        /// <summary>
        /// 可使用数量
        /// </summary>
        public int Quantity { get; set; }
    }
}