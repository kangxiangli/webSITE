
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberModuleConfiguration : EntityConfigurationBase<MemberModule, int>
    {
        public MemberModuleConfiguration()
        {
            ToTable("A_MemberModule");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

