   
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体配置
    /// </summary>
    public class ModuleConfiguration : EntityConfigurationBase<Module, int>
    {
        public ModuleConfiguration() {
            ToTable("A_Module");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Adminis).WithMany(m => m.Modules).Map(m =>
            {

                m.ToTable("A_Module_Administrator_Relation");
                m.MapLeftKey("ModuleId");
                m.MapRightKey("AdministratorId");
            });
        }
    }
}
