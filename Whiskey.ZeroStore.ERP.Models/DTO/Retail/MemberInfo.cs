using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class MemberInfo
    {
        public int MemberId { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public string MemberNum { get; set; }
        /// <summary>
        /// 搭配师编号
        /// </summary>
        public string CollNum { get; set; }
    }
}
