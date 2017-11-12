using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Authorities
{
    public class ARolePermissionRelationConfiguration : EntityConfigurationBase<ARolePermissionRelation, int>
    {
        public ARolePermissionRelationConfiguration()
        {
            ToTable("A_Role_Permission_Relation");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasKey(t => new { t.Id, t.RoleId, t.PermissionsId });
        }
    }
}
