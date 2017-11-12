using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Warehouses
{
    /// <summary>
    /// 检验数据
    /// </summary>
    public class CheckResultEdo
    {
        public CheckResultEdo()
        {
            Valids = new List<string>();
        }

        /// <summary>
        /// 唯一编码
        /// </summary>
        public string UniqueCode { get; set; }

        /// <summary>
        /// 无效数量
        /// </summary>
        public int InvalidCount { get; set; }

        /// <summary>
        /// 有效数量
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// 有效款号集合
        /// </summary>
        public List<string> Valids { get; set; }
    }
}
