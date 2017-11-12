using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    public enum SingleProductFlag
    {
        /// <summary>
        /// 用户上传
        /// </summary>
        Upload = 0,
        /// <summary>
        /// 购买过
        /// </summary>
        OrderItem = 1,

        /// <summary>
        /// 推荐
        /// </summary>
        Recommend =2,
        All =  3
        
    }
}
