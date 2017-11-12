using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Warehouses
{
    public class OrderblankAuditConfiguration : EntityConfigurationBase<OrderblankAudit,int>
    {
        public OrderblankAuditConfiguration()
        {
            ToTable("W_OrderblankAudit");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
