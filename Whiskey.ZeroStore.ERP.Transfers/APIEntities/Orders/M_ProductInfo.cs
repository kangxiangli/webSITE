using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Orders
{
    public class M_ProductInfo
    {
        /// <summary>
        /// 商品
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// 会员
        /// </summary>
        public int MemberId { get; set; }

        ///// <summary>
        ///// 颜色
        ///// </summary>
        //public string ColorId { get; set; }

        ///// <summary>
        ///// 尺码
        ///// </summary>
        //public string SizeId { get; set; }

        ///// <summary>
        ///// 地址
        ///// </summary>
        //public string AddressId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; }
    }
}
