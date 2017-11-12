using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberCollRelationConfiguration : EntityConfigurationBase<MemberCollRelation, int>
    {
        public MemberCollRelationConfiguration()
        {
            ToTable("M_Member_Coll_Relation");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
