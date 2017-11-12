
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PartnerStatisticsConfiguration : EntityConfigurationBase<PartnerStatistics, int>
    {
        public PartnerStatisticsConfiguration()
        {
            ToTable("O_PartnerStatistics");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

