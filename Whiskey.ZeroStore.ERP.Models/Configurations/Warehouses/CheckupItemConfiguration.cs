using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Warehouses
{
    public class CheckupItemConfiguration : EntityConfigurationBase<CheckupItem, int>
    {
        public CheckupItemConfiguration() {
            ToTable("W_CheckupItem");
            Property(c=>c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }

       
    }
}
