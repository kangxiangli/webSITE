using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory
{
    public class SampleEdo
    {

        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 品牌名称
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// 品类名称
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 供货价
        /// </summary>
        public float WholesalePrice { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string OriginalPath { get; set; }


        public int State { get; set; }

        public string Notes { get; set; }
    }
}
