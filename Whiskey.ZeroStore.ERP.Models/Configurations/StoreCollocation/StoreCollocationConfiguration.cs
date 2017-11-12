using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Properties
{
  public  class StoreCollocationConfiguration : EntityConfigurationBase<StoreProductCollocation, int>
    {
        public StoreCollocationConfiguration()
        {
            ToTable("S_StoreCollocation");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
