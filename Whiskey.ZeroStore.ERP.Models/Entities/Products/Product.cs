using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class Product : EntityBase<int>
    {
        public Product()
        {
            ProductImages = new List<ProductImage>();
            ProductOperationLogs = new List<ProductOperationLog>();
            ProductBarcodeDetails = new List<ProductBarcodeDetail>();
            AppointmentFeedbacks = new List<AppointmentFeedback>();
            Inventories = new List<Inventory>();
        }

        /// <summary>
        /// 商品款号=品牌+辅助号+品类
        /// </summary>
        [Display(Name = "商品款号")]
        [StringLength(10)]
        [Index(IsClustered = false, IsUnique = false)]
        public virtual string BigProdNum { get; set; }

        /// <summary>
        /// 原始款号=出厂商吊牌上的款号
        /// </summary>
        [Display(Name = "原始款号")]
        [StringLength(20)]
        public virtual string OriginNumber { get; set; }

        /// <summary>
        /// 商品货号=商品款号+颜色+尺码
        /// </summary>
        [Display(Name = "商品货号")]
        [StringLength(14)]
        [Index(IsClustered = false, IsUnique = true)]
        public virtual string ProductNumber { get; set; }

        [Display(Name = "商品尺码")]
        public virtual int SizeId { get; set; }

        [Display(Name = "商品颜色")]
        public virtual int ColorId { get; set; }

        [Display(Name = "商品类型")]
        public virtual int ProductType { get; set; }//0：直营档案，1：第三方档案

        [Display(Name = "商品名称")]
        [StringLength(20)]
        public virtual string ProductName { get; set; }

        [Display(Name = "商品主图")]
        [StringLength(100)]
        public virtual string OriginalPath { get; set; }

        [Display(Name = "缩略图片")]
        [StringLength(100)]
        public virtual string ThumbnailPath { get; set; }

        [Display(Name = "商品搭配图")]
        [StringLength(100)]
        public virtual string ProductCollocationImg { get; set; }

        [ForeignKey("SizeId")]
        public virtual Size Size { get; set; }

        [ForeignKey("ColorId")]
        public virtual Color Color { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 条码打印数量
        /// </summary>
        public int BarcodePrintCount { get; set; }

        public virtual int? BarcodePrintInfoId { get; set; }

        [ForeignKey("BarcodePrintInfoId")]
        public virtual ProductBarcodePrintInfo BarcodePrintInfo { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; }

        public virtual ICollection<ProductBarcodeDetail> ProductBarcodeDetails { get; set; }

        public virtual ICollection<ProductOperationLog> ProductOperationLogs { get; set; }

        public virtual ProductOriginNumber ProductOriginNumber { get; set; }
        public virtual ICollection<SaleAutoGen> SaleAutoGens { get; set; }
        public virtual ICollection<AppointmentFeedback> AppointmentFeedbacks { get; set; }

        public virtual ICollection<AppointmentGen> AppointmentGens { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }

    }
}


