using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 统计类型
    /// </summary>
    public enum StatTypeEnum
    {
        /// <summary>
        /// 销售统计
        /// </summary>
        SaleStat = 0,

        /// <summary>
        /// 退货统计
        /// </summary>
        ReturnStat = 1
    }

    /// <summary>
    /// 会员统计类型
    /// </summary>
    public enum MemberStatTypeEnum
    {
        会员类型 = 0,

        /// <summary>
        /// 暂未提供
        /// </summary>
        会员热度 = 1, 
        会员性别 = 2,
        尺码 = 3,
        偏好颜色 = 4,
        身高 = 5,
        体重 = 6,
        肩宽 = 7,
        胸围 = 8,
        腰围 = 9,
        臀围 = 10,
        等级 = 11,
        体型 = 12

    }
}
