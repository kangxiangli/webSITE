using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberSingleProductConfiguration : EntityConfigurationBase<MemberSingleProduct, int>
    {
        public MemberSingleProductConfiguration()
        {
            ToTable("P_Member_SingleProduct");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
