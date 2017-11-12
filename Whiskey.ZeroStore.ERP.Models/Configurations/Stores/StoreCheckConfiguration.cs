using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreCheckConfiguration : EntityConfigurationBase<StoreCheckItem, int>
    {
        public StoreCheckConfiguration()
        {
            ToTable("S_StoreCheck");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
