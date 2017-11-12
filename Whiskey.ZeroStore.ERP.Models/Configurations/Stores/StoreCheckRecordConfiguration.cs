using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreCheckRecordConfiguration : EntityConfigurationBase<StoreCheckRecord, int>
    {
        public StoreCheckRecordConfiguration()
        {
            ToTable("S_StoreCheckRecord");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
