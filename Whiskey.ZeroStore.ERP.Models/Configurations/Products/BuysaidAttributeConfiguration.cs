using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Products
{
    public class BuysaidAttributeConfiguration:EntityConfigurationBase<BuysaidAttribute, int>
    {
        public BuysaidAttributeConfiguration()
        {
            ToTable("P_BuysaidAttribute");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
