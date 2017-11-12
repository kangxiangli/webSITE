using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberActivityConfiguration : EntityConfigurationBase<MemberActivity, int>
    {
        public MemberActivityConfiguration()
        {
            ToTable("M_Member_Activity");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.MemberTypes).WithMany(m => m.MemberActivitys).Map(m =>
            {
                m.ToTable("M_MemberActivity_MemberType_Relation");
                m.MapLeftKey("MemberActivityId");
                m.MapRightKey("MemberTypeId");
            });
        }
    }
}
