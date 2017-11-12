using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Products
{
    public class ProductBarcodeDetail : EntityBase<int>
    {
        public int? ProductId { get; set; }
        [StringLength(15)]
        public string ProductNumber { get; set; }
        /// <summary>
        /// 36进制 3位数
        /// </summary>
        [StringLength(3)]
        public string OnlyFlag { get; set; }
        public int OnlfyFlagOfInt { get; set; }
        /// <summary>
        /// 32位guid值
        /// </summary>
        [StringLength(36)]
        public string LogFlag { get; set; }
        /// <summary>
        /// 0：未使用 1：已入库 3：废除  参看ProductBarcodeDetailFlag
        /// </summary>
        public int Status { get; set; }
        [StringLength(100)]
        public string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public Administrator Administrator { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        public virtual ICollection<ProductOperationLog> ProductOperationLogs { get; set; }
    }
}
