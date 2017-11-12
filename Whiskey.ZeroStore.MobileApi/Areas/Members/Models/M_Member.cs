using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Models
{
    public class M_Member
    {
        /// <summary>
        /// 会员标识
        /// </summary>
        public int MemberId { get; set; }                  

        /// <summary>
        /// 真是姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 身份证号码
        /// </summary>
        public string IDCard { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime DateofBirth { get; set; }
       
    }
}