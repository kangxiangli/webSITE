using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberHeatConfiguration : EntityConfigurationBase<MemberHeat, int>
    {
        public MemberHeatConfiguration()
        {
            ToTable("M_Member_Heat");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
