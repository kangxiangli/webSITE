




using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体配置
    /// </summary>
    public class ProductConfiguration : EntityConfigurationBase<Product, int>
    {
        public ProductConfiguration() {
            ToTable("P_Product");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasMany(m => m.ProductImages).WithMany(m => m.Products).Map(m => m.ToTable("P_Product_ProductImages_Relation"));
        }
    }
}
