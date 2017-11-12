using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreTypeConfiguration : EntityConfigurationBase<StoreType, int>
    {
        public StoreTypeConfiguration()
        {
            ToTable("S_StoreType");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
