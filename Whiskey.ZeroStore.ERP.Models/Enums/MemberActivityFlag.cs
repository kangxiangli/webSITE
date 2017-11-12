using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 储值/积分变动类型枚举
    /// </summary>
    public enum MemberActivityFlag
    {

        /// <summary>
        /// 储值=0
        /// </summary>
        [Description("储值")]
        Recharge = 0,

        /// <summary>
        /// 积分=1
        /// </summary>
        [Description("积分")]
        Score = 1,
    }
}
