
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ProductCrowdConfiguration : EntityConfigurationBase<ProductCrowd, int>
    {
        public ProductCrowdConfiguration()
        {
            ToTable("P_ProductCrowd");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

