using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ProductOrigNumberProductDetailConfiguration : EntityConfigurationBase<ProdcutOrigNumberProductDetail, int>
    {
        public ProductOrigNumberProductDetailConfiguration()
        {
            ToTable("P_ProductOrigNumber_ProductDetail");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
