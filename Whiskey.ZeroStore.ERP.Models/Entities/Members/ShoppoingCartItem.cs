using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{

    public class ShoppingCartItem : EntityBase<int>
    {

        public int MemberId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        /// <summary>
        /// 商品id
        /// </summary>
        public int ProductId { get; set; }


        /// <summary>
        /// 商品货号
        /// </summary>
        [Index][StringLength(maximumLength:20)]
        public string ProductNumber { get; set; }

 
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

  
    }
    public class ShoppingCartUpdateDto
    {
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public int Quantity { get; set; }
    }


    public class ShoppingCartEntry
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public int? SaleCampId { get; set; }
        public int? BrandDiscountId { get; set; }
        public int Quantity { get; set; }
        public string ThumbnailPath { get; set; }
        public decimal TagPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public string CreatedTime { get; set; }
        public string BrandName { get; set; }
        public string ColorName { get; set; }
        public string SizeName { get; set; }
        public string CategoryName { get; set; }
    }

    public class ProdutDictEntry
    {
        public string BrandName { get; set; }

        public string ProductNumber { get; set; }
        public decimal TagPrice { get; set; }
        public string BigProdNum { get; set; }

        public decimal RetailPrice { get; set; }
        public int? SaleCampId { get; set; }
        public int? BrandDiscountId { get; set; }
    }


    /// <summary>
    /// ShoppingCartItem 映射表配置
    /// </summary>
    public class ShoppingCartItemConfiguration : EntityConfigurationBase<ShoppingCartItem, int>
    {
        public ShoppingCartItemConfiguration()
        {
            ToTable("M_ShoppingCartItem");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }



}
