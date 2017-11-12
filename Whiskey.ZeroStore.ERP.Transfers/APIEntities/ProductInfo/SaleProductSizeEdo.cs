using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory
{
    public class SaleProductSizeEdo
    {
        /// <summary>
        /// 尺码标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 尺码名称
        /// </summary>
        public string SizeName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
