using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体映射配置
    /// </summary>
    public class ProductDiscountConfiguration : EntityConfigurationBase<ProductDiscount, int>
    {
        public ProductDiscountConfiguration()
        {
            ToTable("P_Product_Discount");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(c => c.Brands).WithMany(c => c.Discounts).Map(m => {
                m.ToTable("P_ProductDiscount_Brand_Relation");
                m.MapLeftKey("ProductDiscountId");
                m.MapRightKey("BrandId");
            });

        }
    }
}
