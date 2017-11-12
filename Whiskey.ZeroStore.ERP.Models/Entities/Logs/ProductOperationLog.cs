using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Warehouses
{
    //yxk 2016-3
    /// <summary>
    /// 商品操作日志
    /// </summary>
    public class ProductOperationLog : EntityBase<int>
    {
        /// <summary>
        /// 商品货号
        /// </summary>
        [StringLength(15)]
        public string ProductNumber { get; set; }
        /// <summary>
        /// 商品一维码
        /// </summary>
        [StringLength(18)]
        public string ProductBarcode { get; set; }
        /// <summary>
        /// 商品一维码后三位唯一标识符
        /// </summary>
        [StringLength(3)]
        public string OnlyFlag { get; set; }
        [StringLength(100)]
        public string Description { get; set; }
        /// <summary>
        /// 32位的guid值
        /// </summary>
        [StringLength(36)]
        public string LogFlag { get; set; }
        
        [ForeignKey("OperatorId")]
        public Administrator Operator { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<ProductBarcodeDetail> ProdutBarcodeDetails { get; set; }
    }
}
