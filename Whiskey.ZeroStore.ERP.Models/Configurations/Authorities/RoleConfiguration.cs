
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
    public class RoleConfiguration : EntityConfigurationBase<Role, int>
    {
        public RoleConfiguration() {
            ToTable("A_Role");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            //HasMany(m => m.Permissions).WithMany(m => m.Roles).Map(m =>
            //{
            //    m.ToTable("A_Role_Permission_Relation");
            //    m.MapLeftKey("RoleId");
            //    m.MapRightKey("PermissionId");
            //});
        }
    }
}
