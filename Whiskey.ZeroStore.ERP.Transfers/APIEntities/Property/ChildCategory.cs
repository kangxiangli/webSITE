using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Property
{
    /// <summary>
    /// 子类品类
    /// </summary>
    public class ChildCategory:BaseCategory
    {      
        /// <summary>
        /// 品类集合
        /// </summary>
        public List<BaseCategory> Categories { get; set; }
    }
}
