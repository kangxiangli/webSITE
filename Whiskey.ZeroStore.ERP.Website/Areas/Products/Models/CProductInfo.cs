using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Models
{
    public class CProductInfo
    {
        public CProductInfo()
        {
            ProductNumbs = new List<ProductNumbs>();
            ProductImages = new List<ProductImage>();
        }
        /// <summary>
        /// 品牌ID
        /// </summary>
        public int BrandId { get; set; }
        /// <summary>
        /// 品类ID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// 季节ID
        /// </summary>
        public int SeasonId { get; set; }
        /// <summary>
        /// 面向人群
        /// </summary>
        public int CrowdId { get; set; }

        /// <summary>
        /// 原始款号
        /// </summary>
        public string OrignNum { get; set; }
        /// <summary>
        /// 商品款号
        /// </summary>
        public string ProduNum { get; set; }
        /// <summary>
        /// 吊牌价格
        /// </summary>
        public float Tagprice { get; set; }
        /// <summary>
        /// 进货价
        /// </summary>
        public float WholesalePrice { get; set; }
        /// <summary>
        /// 采购价
        /// </summary>
        public float PurchasePrice { get; set; }

        /// <summary>
        /// 商品备注
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// 新增的商品货号
        /// </summary>
        public List<ProductNumbs> ProductNumbs { get; set; }
        /// <summary>
        /// 商品标题
        /// </summary>
        public string ProduTit { get; set; }
        /// <summary>
        /// 促销标题
        /// </summary>
        public string SalesTitle { get; set; }
        /// <summary>
        /// 模板ID
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 手机模板Id
        /// </summary>
        public int TemplatePhoneId { get; set; }
        /// <summary>
        /// 商品搭配图
        /// </summary>
        public string ProductCollocationImg { get; set; }
        /// <summary>
        /// 商品主图
        /// </summary>
        public string OriginalPath { get; set; }
        /// <summary>
        /// 原始款号明细图(共用)
        /// </summary>
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        /// <summary>
        /// 图片属性id 如：  ，1，2，3，
        /// </summary>
        public string PicAttIds { get; set; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string ProDescr { get; set; }
        /// <summary>
        /// 其他的相关搭配
        /// </summary>
        public string OtherCollo { get; set; }
        /// <summary>
        /// 买手说
        /// </summary>
        public string Buysaid { get; set; }
        /// <summary>
        /// 买手说标签  如 ，2，3，4，
        /// </summary>
        public string BuysaidAttrId { get; set; }
        /// <summary>
        /// 其他保养维护
        /// </summary>
        public string MaintainIds { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        public string JumpLink { get; set; }

        /// <summary>
        /// 吊牌属性
        /// </summary>
        public virtual ProductOriginNumberTag ProductOriginNumberTag { get; set; }
    }

    public class BarcodeTem
    {
        public string ProductNumber { get; set; }
        public string[] LastCode { get; set; }
    }
    public class ProductNumbs
    {
        /// <summary>
        /// 商品货号
        /// </summary>
        public string ProductNumber { get; set; }
        /// <summary>
        /// 商品搭配图
        /// </summary>
        public string ProductCollocationImg { get; set; }
        /// <summary>
        /// 商品主图
        /// </summary>
        public string OriginalPath { get; set; }
        /// <summary>
        /// 明细图
        /// </summary>
        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}