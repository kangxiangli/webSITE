using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    /// <summary>
    /// 会员状态枚举类
    /// </summary>
    public class MemberStatus
    {
        /// <summary>
        /// 会员注册来源
        /// </summary>
        public enum RegisterType 
        {
            /// <summary>
            /// 手动注册
            /// </summary>
            ManualRegister=0,
            /// <summary>
            /// 网站注册
            /// </summary>
            WebsiteRegister=1,
        }
        /// <summary>
        /// 会员类型
        /// </summary>
        public enum MemberType
        { 
            /// <summary>
            /// 普通会员 
            /// </summary>
            OrdinaryMember=0,
            /// <summary>
            /// VIP会员
            /// </summary>
            VIPMember=1,
            /// <summary>
            /// 折扣会员（永远3折）
            /// </summary>
            DiscountMember=2,
        }

        

        /// <summary>
        /// 会员活动类型
        /// </summary>
        public enum MemberActivityType
        {
            /// <summary>
            /// 办卡充值
            /// </summary>
            Recharge=0,
            
            /// <summary>
            /// 其他活动（送积分）
            /// </summary>
            Activity=1,
        }
        /// <summary>
        /// 会员活动状态
        /// </summary>
        public enum MemberActivityStatus
        {
            /// <summary>
            /// 普通，在一定时间内有效
            /// </summary>
            Ordinary =0,
            /// <summary>
            /// 永久有效
            /// </summary>
            Forever=1,
            
        }

        /// <summary>
        /// 会员充值类型
        /// </summary>
        public enum MemberDepositType
        {
            /// <summary>
            /// 系统充值
            /// </summary>
            System=0,
            /// <summary>
            /// 人工充值
            /// </summary>
            Manpower =1,
        }
    }
}
