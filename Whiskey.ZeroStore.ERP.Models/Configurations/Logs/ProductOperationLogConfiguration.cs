using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Logs
{
    public class ProductOperationLogConfiguration : EntityConfigurationBase<ProductOperationLog, int>
    {
        public ProductOperationLogConfiguration()
        {
            ToTable("L_ProductOperationLog");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(m => m.Products).WithMany(m => m.ProductOperationLogs).Map(m =>
            {
                m.ToTable("L_ProductLogs_Product_Relation");
                m.MapLeftKey("LogId");
                m.MapRightKey("ProductId");
            });
            HasMany(m => m.Inventories).WithMany(m => m.ProductOperationLogs).Map(m =>
            {
                m.ToTable("L_ProductLogs_Inventory_Relation");
                m.MapLeftKey("LogId");
                m.MapRightKey("InventoryId");
            });

            HasMany(m => m.ProdutBarcodeDetails).WithMany(m => m.ProductOperationLogs).Map(m =>
            {
                m.ToTable("L_ProductLogs_BarcodeDetail_Relation");
                m.MapLeftKey("LogId");
                m.MapRightKey("ProductDetailId");
            });

        }
    }
}
