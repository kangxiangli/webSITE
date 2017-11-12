using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Products
{
    public class ProductNumberAttrConfiguration : EntityConfigurationBase<ProductBigNumberAttr, int>
    {
        public ProductNumberAttrConfiguration() {
            ToTable("P_ProductBigNumberAttr");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
