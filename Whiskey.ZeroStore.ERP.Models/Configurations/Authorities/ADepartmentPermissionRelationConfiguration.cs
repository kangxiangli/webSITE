using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Authorities
{
    public class ADepartmentPermissionRelationConfiguration : EntityConfigurationBase<ADepartmentPermissionRelation, int>
    {
        public ADepartmentPermissionRelationConfiguration()
        {
            ToTable("A_Department_Permissions_Relation");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasKey(t => new { t.Id, t.DepartmentId, t.PermissionsId });
        }
    }
}
