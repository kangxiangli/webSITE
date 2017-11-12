using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
 
namespace Whiskey.ZeroStore.ERP.Models
{
    public class ApprovalConfiguration : EntityConfigurationBase<Approval, int>
    {
        public ApprovalConfiguration()
        {
            ToTable("C_Approval");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
