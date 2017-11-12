using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Collocations;


namespace Whiskey.ZeroStore.ERP.Models.Configurations.Collocations
{
    class EarningsDetailConfiguration:EntityConfigurationBase<EarningsDetail,int>
    {
        public EarningsDetailConfiguration() {
            ToTable("S_CollocationEarningDetail");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
