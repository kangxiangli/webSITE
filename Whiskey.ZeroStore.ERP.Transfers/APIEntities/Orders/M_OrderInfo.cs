using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Orders
{
    public class M_OrderInfo
    {
        /// <summary>
        /// 会员
        /// </summary>
        public int MemberId {get;set;}

        /// <summary>
        /// 收货地址
        /// </summary>
        public  string AddressId {get;set;}

        /// <summary>
        /// IP
        /// </summary>
        public  string Ip {get;set;}

        /// <summary>
        /// 订单来源
        /// </summary>
        public string OrderSource{get;set;}

        /// <summary>
        /// 是否来自购物车
        /// </summary>
        public string IsCart { get; set; }

        /// <summary>
        /// 支付类型
        /// </summary>
        public string PayType { get; set; }
    }
}
