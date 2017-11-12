using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Member
{

    /// <summary>
    /// Api-会员更新标识
    /// </summary>
    public enum MemberUpdateFlag
    {
        /// <summary>
        /// 昵称
        /// </summary>
        MemberName,

        /// <summary>
        /// 手机号码
        /// </summary>
        MobilePhone,

        /// <summary>
        /// 密码
        /// </summary>
        Password,

        /// <summary>
        /// 邮箱
        /// </summary>
        Email,

        /// <summary>
        /// 性别
        /// </summary>
        Gender,

        /// <summary>
        /// 身份证
        /// </summary>
        IDCard,

        /// <summary>
        /// 生日
        /// </summary>
        Birthday,

        /// <summary>
        /// 真实姓名
        /// </summary>
        RealName,
    }
}
