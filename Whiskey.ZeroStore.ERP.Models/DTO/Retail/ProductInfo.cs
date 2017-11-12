using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 零售订单中商品明细项dto
    /// </summary>
    public class ProductInfo: MiniProductInfo
    {

        /// <summary>
        /// 商品活动折扣类型
        /// </summary>
        public SalesCampaignType? CampType { get; set; }

        /// <summary>
        /// 折扣,打几折
        /// </summary>
        public float? CampDiscount { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string[] Barcodes { get; set; }

    }

    public class MiniProductInfo
    {
        /// <summary>
        /// 商品货号
        /// </summary>
        public string ProdNum { get; set; }

        /// 商品数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 商品活动id
        /// </summary>
        public int? CampId { get; set; }
 

    }
}
