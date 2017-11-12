using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models.Configurations
{
    public class ProductOrigNumberConfiguration : EntityConfigurationBase<ProductOriginNumber, int>
    {
        public ProductOrigNumberConfiguration()
        {
            ToTable("P_ProductOrigNumber");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.BuysaidAttributes).WithMany(m => m.ProductOrigNumbers).Map(m =>
            {
                m.ToTable("P_ProductOrigNumber_BuysaidAttribute_Relation");
                m.MapLeftKey("ProductOrigNumberId");
                m.MapRightKey("BuysaidAttributeId");
            });
            HasMany(m => m.ProductAttributes).WithMany(m => m.ProductOrigNumbers).Map(m =>
            {
                m.ToTable("P_ProductOrigNumber_ProductAttributes_Relation");
                m.MapLeftKey("ProductOrigNumberId");
                m.MapRightKey("ProductAttributeId");
            });

            HasMany(m => m.ProductImages).WithMany(m => m.ProductOrginNumbers).Map(m => m.ToTable("P_ProductOrginNumber_ProductImages_Relation"));
        }
    }
}
