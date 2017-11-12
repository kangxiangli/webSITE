using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreLevelConfiguration : EntityConfigurationBase<StoreLevel, int>
    {
        public StoreLevelConfiguration()
        {
            ToTable("S_StoreLevel");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
