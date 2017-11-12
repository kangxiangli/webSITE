
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
    public class AdministratorConfiguration : EntityConfigurationBase<Administrator, int>
    {
        public AdministratorConfiguration() {
            ToTable("A_Administrator");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Roles).WithMany(m => m.Administrators).Map(m =>
            {
                m.ToTable("A_Administrator_Role_Relation");
                m.MapLeftKey("AdministratorId");
                m.MapRightKey("RoleId");
            });
            //HasMany(m => m.Permissions).WithMany(m => m.Adminis).Map(m =>
            //{
            //    m.ToTable("A_Administrator_Permission_Relation");
            //    m.MapLeftKey("AdministratorId");
            //    m.MapRightKey("PermissionId");
            //});
        }
    }
}
