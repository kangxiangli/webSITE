using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AdjustDepositConfiguration : EntityConfigurationBase<AdjustDeposit, int>
    {
        public AdjustDepositConfiguration()
        {
            ToTable("M_Adjust_Deposit");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);             
        }
    }
}
