using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models.Configurations
{
    public class ProductOriginNumberTagConfiguration : EntityConfigurationBase<ProductOriginNumberTag, int>
    {
        public ProductOriginNumberTagConfiguration()
        {
            ToTable("P_ProductOrigNumberTag");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
