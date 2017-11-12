using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Property
{
    /// <summary>
    /// 基类品类
    /// </summary>
    public class BaseCategory
    {

        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 品类名称
        /// </summary>
        public string CategoryName { get; set; }
    }
}
