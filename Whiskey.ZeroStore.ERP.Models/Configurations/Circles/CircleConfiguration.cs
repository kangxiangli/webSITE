using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class CircleConfiguration : EntityConfigurationBase<Circle, int>
    {
        public CircleConfiguration()
        {
            ToTable("M_Circle");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Members).WithMany(m => m.Circles).Map(m =>
            {
                m.ToTable("M_Member_Circle_Relation");
                m.MapLeftKey("MemberId");
                m.MapRightKey("CircleId");
            });
        }
    }
}
