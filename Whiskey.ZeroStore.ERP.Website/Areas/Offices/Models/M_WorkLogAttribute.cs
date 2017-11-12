using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models
{
    public class M_WorkLogAttribute
    {
        public M_WorkLogAttribute()
        {
            Children = new List<M_WorkLogAttribute>();
        }
        /// <summary>
        /// 当Id为null时表示父级，Id不为null时为子集
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string WorkLogAttributeName { get; set; }

        /// <summary>
        /// 子集
        /// </summary>
        public IEnumerable<M_WorkLogAttribute> Children { get; set; }
    }
}