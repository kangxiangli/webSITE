using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Properties
{
    public class ScoreRuleConfiguration : EntityConfigurationBase<ScoreRule,int>
    {
        public ScoreRuleConfiguration() {
            ToTable("P_ScoreRule");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
