using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    //yxk  2015-10-28 model
    public class CheckDto_t
    {
        public String ProductNumber { get; set; }
        public int Quantity { get; set; }
    }
    public class CheckedType
    {
        /// <summary>
        /// 标识符
        /// </summary>
        public string UUID { get; set; }//标识符

        /// <summary>
        /// 盘点店铺
        /// </summary>
        public string StoreId { get; set; }//盘点店铺

        /// <summary>
        /// 盘点仓库
        /// </summary>
        public string StorageId { get; set; } //盘点仓库

        /// <summary>
        /// 参看枚举CheckerItemFlag
        /// </summary>
        public int Resultype { get; set; }

        /// <summary>
        /// 当无效状态时的校验码，参考InvalidFlag
        /// </summary>
        public int InvalidType { get; set; }

        /// <summary>
        /// 待盘数量
        /// </summary>
        public int CheckQuantity { get; set; } //待盘数量

        /// <summary>
        /// 已盘数量
        /// </summary>
        public int CheckedQuantity { get; set; } //已盘数量

        /// <summary>
        /// 有效数量
        /// </summary>
        public int ValidQuantity { get; set; } //有效数量

        /// <summary>
        /// 无效数量
        /// </summary>
        public int InvalidQuantity { get; set; } //无效数量

        /// <summary>
        /// 缺货数量
        /// </summary>
        public int MissingQuantity { get; set; } //缺货数量

        /// <summary>
        /// 余货数量
        /// </summary>
        public int ResidueQuantity { get; set; } //余货数量

        /// <summary>
        /// 品牌ID
        /// </summary>
        public int BrandId { get; set; } 

        /// <summary>
        /// 款式Id
        /// </summary>
        public int CategoryId { get; set; } 

        /// <summary>
        /// 季节ID
        /// </summary>
        public int SeasonId { get; set; } 
        
        /// <summary>
        /// 颜色
        /// </summary>
        public int ColorId { get; set; } 

        /// <summary>
        /// 尺码
        /// </summary>
        public int SizeId { get; set; } 
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 传递其他的状态标识
        /// </summary>
        public int OtherInfo { get; set; }//传递其他的状态标识
        

        //public int BackOutCount { get; set; }//撤销数量
    }
    public class RetuCheckData
    {
        public int Id { get; set; }
        public string ProductNumber { get; set; }
        public string Thumbnail { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Count { get; set; }
    }
}