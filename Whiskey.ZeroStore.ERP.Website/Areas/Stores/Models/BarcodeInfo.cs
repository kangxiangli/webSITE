using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class BarcodeInfo
    {
        public string ProductNumber { get; set; }
        /// <summary>
        /// 打印总数
        /// </summary>
        public int PrintCount { get; set; }
        public int MaxCount { get; set; }
        /// <summary>
        /// 起始标记
        /// </summary>
        public string StartFlag { get; set; }
        public string Other { get; set; }

    }
    /// <summary>
    /// 重复打印条形码用
    /// </summary>
    public class BarcodeOnlyInfo
    {
        public BarcodeOnlyInfo()
        {
            BarcodeInfos = new List<BarcodeInfo>();
        }
        public string ProductNumber { get; set; }

        /// <summary>
        /// 其中的ProductNumber是条形码,不是商品货号
        /// </summary>
        public List<BarcodeInfo> BarcodeInfos { get; set; }
    }

    public class BarcodeInfoBase
    {
       
        public List<string> BarcodeNumbers { get; set; }
        public string QRcode { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public float TagPrice { get; set; }
        public int State { get; set; }


        //-----------40*80用---------

        public string BigProdNum { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 产地
        /// </summary>
        public string ProductionPlace { get; set; }
        /// <summary>
        /// 执行标准
        /// </summary>
        public string Standard { get; set; }
        /// <summary>
        /// 检验员
        /// </summary>
        public string Inspector { get; set; }
        /// <summary>
        /// 面料
        /// </summary>
        public string Fabric { get; set; }
        /// <summary>
        /// 里料
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// 配料
        /// </summary>
        public string batching { get; set; }

        /// <summary>
        /// 制造商
        /// </summary>
        public string Manufacturer { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string PostCode { get; set; }
        /// <summary>
        /// 品名
        /// </summary>
        public string CateName { get; set; }
        /// <summary>
        /// 尺码扩展属性
        /// </summary>
        public string SizeExtention { get; set; }
    }
}