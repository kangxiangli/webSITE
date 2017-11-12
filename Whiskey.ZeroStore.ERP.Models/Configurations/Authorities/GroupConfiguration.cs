using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Authorities
{
    public class GroupConfiguration : EntityConfigurationBase<Group, int>
    {
        public GroupConfiguration() {
            ToTable("A_Group");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            //HasMany(m =>m.Admins).WithMany(m =>m.Groups).Map(m =>
            //{
            //    m.ToTable("A_Group_Administrator_Relation");
            //    m.MapLeftKey("GroupId");
            //    m.MapRightKey("AdministratorId");
            //});
            //HasMany(m => m.Roles).WithMany(m =>m.Groups).Map(m =>
            //{
            //    m.ToTable("A_Group_Role_Relation");
            //    m.MapLeftKey("GroupId");
            //    m.MapRightKey("RoleId");
            //});
            //HasMany(m =>m.Permissions).WithMany(m =>m.Groups).Map(m =>
            //{
            //    m.ToTable("A_Group_Permissions_Relation");
            //    m.MapLeftKey("GroupId");
            //    m.MapRightKey("PermissionsId");
            //});
        }
    }
}
