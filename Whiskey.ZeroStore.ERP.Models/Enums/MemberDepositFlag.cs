using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 会员充值方式
    /// </summary>
    public enum MemberDepositFlag
    {
        /// <summary>
        /// 系统充值
        /// </summary>
        System = 0,
        /// <summary>
        /// 人工充值
        /// </summary>
        Manpower = 1
    }


    /// <summary>
    /// 赠送标识
    /// </summary>
    public enum RewardFlag
    {
        /// <summary>
        /// 积分
        /// </summary>
        Score = 0,

        /// <summary>
        /// 钱
        /// </summary>
        Money = 1,

    }
}
