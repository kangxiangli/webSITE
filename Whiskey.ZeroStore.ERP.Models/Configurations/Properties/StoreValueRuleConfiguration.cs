using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Properties
{
   public  class StoreValueRuleConfiguration : EntityConfigurationBase<StoreValueRule, int>
    {
        public StoreValueRuleConfiguration()
        {
            ToTable("F_StoreValueRule");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.MemberTypes).WithMany(m => m.StoreValueRules).Map(m =>
            {
                m.ToTable("F_StoreValueRule_MemberType_Relation");
                m.MapLeftKey("StoreValueRuleId");
                m.MapRightKey("MemberTypeId");
            });
        }
    }
}
