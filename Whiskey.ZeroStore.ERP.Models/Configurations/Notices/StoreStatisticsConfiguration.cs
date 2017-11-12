using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Notices
{
    public class StoreStatisticsConfiguration : EntityConfigurationBase<StoreStatistics, int>
    {
        public StoreStatisticsConfiguration()
        {
            ToTable("M_StoreStatistics");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
