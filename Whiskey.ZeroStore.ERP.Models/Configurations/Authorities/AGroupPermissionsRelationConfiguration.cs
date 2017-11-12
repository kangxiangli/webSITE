using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Authorities
{
    public class AGroupPermissionsRelationConfiguration : EntityConfigurationBase<AGroupPermissionRelation, int>
    {
        public AGroupPermissionsRelationConfiguration()
        {
            ToTable("A_Group_Permissions_Relation");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasKey(t => new { t.Id, t.GroupId, t.PermissionsId });
        }
    }
}
