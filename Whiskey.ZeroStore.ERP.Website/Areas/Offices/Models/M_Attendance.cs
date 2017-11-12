using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models
{
    public class M_Attendance
    {

        public int Id { get; set; }
        /// <summary>
        /// 员工
        /// </summary>
        public int AdminId { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 次数
        /// </summary>
        public int Count { get; set; }
    }
}