using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PartnerManageCheckConfiguration : EntityConfigurationBase<PartnerManageCheck, int>
    {
        public PartnerManageCheckConfiguration()
        {
            ToTable("O_PartnerManageCheck");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
