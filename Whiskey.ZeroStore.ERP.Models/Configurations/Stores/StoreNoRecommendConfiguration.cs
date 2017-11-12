using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Stores
{
    public class StoreNoRecommendConfiguration : EntityConfigurationBase<StoreNoRecommend, int>
    {
        public StoreNoRecommendConfiguration()
        {
            ToTable("S_StoreNoRecommend");
            Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
