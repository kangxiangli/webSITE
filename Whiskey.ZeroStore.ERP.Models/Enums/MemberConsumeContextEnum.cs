using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 会员消费上下文,消费类型,
    /// </summary>
    public enum MemberConsumeContextEnum
    {
        /// <summary>
        /// 包含微信支付等线上支付
        /// </summary>
        线上消费 = 0,

        /// <summary>
        /// 包含线下零售时支付
        /// </summary>
        线下消费 = 1,

        /// <summary>
        /// 用于后台[储值积分维护]功能
        /// </summary>
        系统调整 = 2,


        /// <summary>
        /// 用于商品退货时,扣除会员获得的赠送积分
        /// </summary>
        退货扣除 = 3,


        /// <summary>
        /// 切换店铺
        /// </summary>

        店铺切换 = 4,


        /// <summary>
        /// 用于补卡时积分扣除
        /// </summary>
        补卡扣除 = 5,


        /// <summary>
        /// 用于请假时积分扣除
        /// </summary>
        请假扣除 = 6,

        /// <summary>
        /// 补考
        /// </summary>
        补考 = 7
    }
}
