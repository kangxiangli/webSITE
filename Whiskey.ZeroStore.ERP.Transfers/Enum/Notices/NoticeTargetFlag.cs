using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Notices
{
    /// <summary>
    /// 通知目标人群类型
    /// </summary>
    public enum NoticeTargetFlag
    {
        /// <summary>
        /// 员工
        /// </summary>
        Admin,
        /// <summary>
        /// 会员
        /// </summary>
        [Obsolete("废弃",true)]
        Member,
        /// <summary>
        /// 部门
        /// </summary>
        Department
    }
}
