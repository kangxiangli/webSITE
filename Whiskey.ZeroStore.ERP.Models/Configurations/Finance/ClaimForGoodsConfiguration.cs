
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ClaimForGoodsConfiguration : EntityConfigurationBase<ClaimForGoods, int>
    {
        public ClaimForGoodsConfiguration()
        {
            ToTable("F_ClaimForGoods");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

