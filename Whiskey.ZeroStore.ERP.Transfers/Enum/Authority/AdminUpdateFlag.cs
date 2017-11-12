using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Authority
{
    /// <summary>
    /// 员工更新标识
    /// </summary>
    public enum AdminUpdateFlag
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        RealName,

        /// <summary>
        /// 手机号码
        /// </summary>
        MobilePhone,

        /// <summary>
        /// 性别
        /// </summary>
        Gender,

        /// <summary>
        /// 邮箱
        /// </summary>
        Email,

        /// <summary>
        /// 出生年月
        /// </summary>
        DateOfBirth,

        /// <summary>
        /// mac地址
        /// </summary>
        MacAddress,

        /// <summary>
        /// 身份证
        /// </summary>
        IDCard
    }
}
