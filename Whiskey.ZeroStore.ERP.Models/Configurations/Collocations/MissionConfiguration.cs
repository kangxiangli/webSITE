using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MissionConfiguration:EntityConfigurationBase<Mission,int>
    {
        public MissionConfiguration()
        {
            ToTable("S_Mission");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Categorys).WithMany(m => m.Missions).Map(m =>
            {
                m.ToTable("S_Missions_Categorys_Relation");
                m.MapLeftKey("MissionId");
                m.MapRightKey("CategoryId");
            });
            HasMany(m => m.MemberColls).WithMany(m => m.Missions).Map(m =>
            {
                m.ToTable("S_Missions_MemberColls_Relation");
                m.MapLeftKey("MissionId");
                m.MapRightKey("MemberCollId");
            });
        }
         
    }
}
