using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Properties
{
    public class MaintainExtendConfiguration : EntityConfigurationBase<MaintainExtend, int>
    {
       public MaintainExtendConfiguration()
       {
           ToTable("P_MaintainExtend");
           Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           HasMany(c => c.Products).WithMany(c => c.MaintainExtends).Map(c =>
           {
               c.ToTable("P_ProductOrigNumber_MaintainExtend_Relation");
               c.MapLeftKey("ProductOrigNumberId");
               c.MapRightKey("MaintainExtendId");
           });
       }
    }
}
