using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PurchaseAuditConfiguration : EntityConfigurationBase<PurchaseAudit, int>
    {
        public PurchaseAuditConfiguration()
        {
            ToTable("W_PurchaseAudit");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
