using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory
{
    public class ProductEdo
    {
        
        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; set; }        

        /// <summary>
        /// 品牌名称
        /// </summary>
        public string BrandName { get; set; }

        /// <summary>
        /// 尺码名称
        /// </summary>
        public string SizeName { get; set; }

        /// <summary>
        /// 尺码数量
        /// </summary>
        public int SizeCount { get; set; }

        /// <summary>
        /// 颜色名称
        /// </summary>
        public string ColorName { get; set; }

        /// <summary>
        /// 颜色数量
        /// </summary>
        public int ColorCount { get; set; }

        /// <summary>
        /// 品类名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string ImagePath { get; set; }
        /// <summary>
        /// 商品款号
        /// </summary>
        public string BigProdNum { get; set; }
    }
}
