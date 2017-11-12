using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    /// <summary>
    /// 零售页会员登陆模式
    /// </summary>
    public enum MemberRetailLoginModeEnum
    {
        /// <summary>
        /// 普通密码登陆
        /// </summary>
        NORMAL_WITH_PASSWORD = 0,

        /// <summary>
        /// APP确认登陆
        /// </summary>
        APP_CONFIRM = 1
    }
}
