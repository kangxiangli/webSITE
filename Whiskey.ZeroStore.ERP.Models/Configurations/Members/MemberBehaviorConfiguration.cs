using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberBehaviorConfiguration : EntityConfigurationBase<MemberBehavior, int>
    {
        public MemberBehaviorConfiguration()
        {
            ToTable("M_MemberBehavior");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
