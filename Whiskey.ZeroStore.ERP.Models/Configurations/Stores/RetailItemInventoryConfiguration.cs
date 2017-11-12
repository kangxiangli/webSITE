using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Stores
{
    public class RetailItemInventoryConfiguration : EntityConfigurationBase<RetailInventory, int>
    {
        public RetailItemInventoryConfiguration()
        {
            ToTable("S_RetailItemInventory");
            Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
