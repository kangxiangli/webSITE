
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
    public class PermissionConfiguration : EntityConfigurationBase<Permission, int>
    {
        public PermissionConfiguration()
        {
            ToTable("A_Permission");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           
            //HasMany(m => m.Departments).WithMany(m =>m.Permissions).Map(m =>
            //{
            //    m.ToTable("A_Permission_Department_Relation");
            //    m.MapLeftKey("PermissionId");
            //    m.MapRightKey("DepartmentId");
            //});
        }
    }
}
