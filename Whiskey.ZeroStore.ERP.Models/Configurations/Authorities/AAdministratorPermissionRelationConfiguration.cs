using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AdministratorPermissionRelationConfiguration : EntityConfigurationBase<AAdministratorPermissionRelation, int>
    {
        public AdministratorPermissionRelationConfiguration()
        {
            ToTable("A_Administrator_Permission_Relation");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasKey(t => new { t.Id, t.AdministratorId, t.PermissionId });
        }
    }
}
