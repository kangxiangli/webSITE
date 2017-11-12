
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberRoleConfiguration : EntityConfigurationBase<MemberRole, int>
    {
        public MemberRoleConfiguration()
        {
            ToTable("M_MemberRole");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.MemberModules).WithMany(m => m.MemberRoles).Map(m =>
            {
                m.ToTable("A_MemberRole_MemberModule_Relation");
                m.MapLeftKey("MemberRoleId");
                m.MapRightKey("MemberModuleId");
            });
        }
    }
}

