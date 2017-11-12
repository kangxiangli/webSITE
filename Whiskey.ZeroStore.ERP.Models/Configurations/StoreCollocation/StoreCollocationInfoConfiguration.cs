using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Properties
{
    public class StoreCollocationInfoConfiguration : EntityConfigurationBase<StoreCollocationInfo, int>
    {
        public StoreCollocationInfoConfiguration()
        {
            ToTable("S_StoreCollocationInfo");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
