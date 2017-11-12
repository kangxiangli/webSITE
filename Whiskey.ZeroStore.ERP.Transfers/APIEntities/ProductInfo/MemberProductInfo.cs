using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.ProductInfo
{
    public class MemberProductInfo
    {
        public MemberProductInfo()
        {
            ProductAttrIds = new List<int>();
        }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }



        /// <summary>
        /// 商品类型 0表示用户上传的，1表示用户购买过的
        /// </summary>
        public int ProductType { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// 品类Id
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        ///  品类名称
        /// </summary>
        public string CategoryName { get; set; }
        
        /// <summary>
        /// 商品价格
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 季节Id
        /// </summary>
        public int? SeasonId { get; set; }

        /// <summary>
        /// 季节名称
        /// </summary>
        public string SeasonName { get; set; }

        /// <summary>
        /// 尺寸Id
        /// </summary>
        public int? SizeId { get; set; }

        /// <summary>
        /// 尺码
        /// </summary>
        public string SizeName { get; set; }

        /// <summary>
        /// 颜色Id
        /// </summary>
        public int? ColorId { get; set; }

        /// <summary>
        /// 颜色名称
        /// </summary>
        public string ColorName { get; set; }

        /// <summary>
        /// 颜色路径
        /// </summary>
        public string ColorIconPath { get; set; }

        /// <summary>
        /// 商品属性(风格)
        /// </summary>
        public List<int> ProductAttrIds { get; set; }
        public string TempProductAttrIds { get; set; }

        /// <summary>
        /// 商品属性(风格)
        /// </summary>
        public string ProductAttrId { get; set; }

        public string ProductAttrName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// 搭配图
        /// </summary>
        public string ImagePath { get; set; }

        public string Notes { get; set; }


        /// <summary>
        /// 款号
        /// </summary>
        public string BigProdNumber { get; set; }
    }
}
