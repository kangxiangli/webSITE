using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberColloEleConfiguration : EntityConfigurationBase<MemberColloEle, int>
    {
        public MemberColloEleConfiguration()
        {
            ToTable("P_Member_Collo_Ele");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);                          
        }

    }
}
