using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberFaceConfiguration : EntityConfigurationBase<MemberFace, int>
    {
        public MemberFaceConfiguration()
        {
            ToTable("M_MemberFace");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
