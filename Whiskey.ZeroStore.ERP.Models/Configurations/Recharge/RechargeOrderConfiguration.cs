using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Recharge
{
   public  class RechargeOrderConfiguration : EntityConfigurationBase<RechargeOrder, int>
    {
        public RechargeOrderConfiguration()
        {
            ToTable("R_RechargeOrde");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
