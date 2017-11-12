using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models.Configurations.Stores
{
    public class SalesCampaignConfiguration : EntityConfigurationBase<SalesCampaign, int>
    {
        public SalesCampaignConfiguration()
        {
            ToTable("P_SalesCampaign");
            Property(c => c.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(c => c.ProductOriginNumbers)
                .WithMany(c => c.SalesCampaigns)
                .Map(m =>
                    {
                        m.ToTable("P_SalesCampaign_ProductOriginNumber_Relation");
                        m.MapLeftKey("SalesCampaignId");
                        m.MapRightKey("ProductOriginNumberId");
                    });
        }
    }
}
