using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ProductAttributeImageConfiguration : EntityConfigurationBase<ProductAttributeImage, int>
    {
        public ProductAttributeImageConfiguration()
        {
            ToTable("P_Product_Attribute_Image");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
