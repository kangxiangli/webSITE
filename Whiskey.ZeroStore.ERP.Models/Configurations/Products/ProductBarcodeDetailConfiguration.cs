using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Models.Configurations
{
    public class ProductBarcodeDetailConfiguration : EntityConfigurationBase<ProductBarcodeDetail, int>
    {
        public ProductBarcodeDetailConfiguration()
        {
            ToTable("P_ProductBarcodeDetail");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           
        }
    }
}
