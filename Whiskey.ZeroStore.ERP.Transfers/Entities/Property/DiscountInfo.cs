using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class DiscountInfo
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        public int Id{get;set;}
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型：0表示没选中，1表示选中
        /// </summary>
        public int Type { get; set; }
    }
}
