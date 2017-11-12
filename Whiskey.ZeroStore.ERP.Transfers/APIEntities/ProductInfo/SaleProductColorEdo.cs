using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory
{
    public class SaleProductColorEdo
    {
        public SaleProductColorEdo()
        {
            SaleProductColorEdos = new List<SaleProductSizeEdo>();
        }

        /// <summary>
        /// 颜色标识
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 颜色图片路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductPath { get; set; }

        /// <summary>
        /// 颜色名称
        /// </summary>
        public string ColorName { get; set; }


        /// <summary>
        /// 颜色下对应的尺码
        /// </summary>
        public ICollection<SaleProductSizeEdo> SaleProductColorEdos { get; set; }
    }
}
