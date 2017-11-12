using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreDepositConfiguration : EntityConfigurationBase<StoreDeposit, int>
    {
        public StoreDepositConfiguration()
        {
            ToTable("S_StoreDeposit");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
