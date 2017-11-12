using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberIMProfileConfig: EntityConfigurationBase<MemberIMProfile,int>
    {
        public MemberIMProfileConfig()
        {
            ToTable("M_MemberIMProfile");
            Property(m => m.Id).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
        }
    }
}
