using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Models
{
    //返回给前台树形结构的数据列表
    //yxk 2015-10-15
    public class ProductTree
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string ProductNumber { get; set; }//产品编号
        public string AssistantNumber { get; set; }//吊牌编码
        public string CategoryName { get; set; } //分类名
        public string BrandName { get; set; } //品牌名
        public string SizeName { get; set; }
        public string SeasonName { get; set; }

        public string ThumbnailPath { get; set; }
        public string ColorName { get; set; }
        public string RGB { get; set; }
        public string TagPrice { get; set; }
        public string RetailPrice { get; set; }
        public string WholesalePrice { get; set; }

        public string PurchasePrice { get; set; }

        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEnabled { get; set; }
        public bool UseDefaultDiscount { get; set; }
        public DateTime UpdateTime { get; set; }
        public string AdminName { get; set; }
    }
}