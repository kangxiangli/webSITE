using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class StoreRecommendDto:IAddDto
    {
        public int StoreId { get; set; }

        /// <summary>
        /// 商城下推荐的大品类名称
        /// </summary>
        public string BigProdNum { get; set; }
    }
}
