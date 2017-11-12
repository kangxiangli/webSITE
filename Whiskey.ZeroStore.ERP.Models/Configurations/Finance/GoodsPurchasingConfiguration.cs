
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class GoodsPurchasingConfiguration : EntityConfigurationBase<GoodsPurchasing, int>
    {
        public GoodsPurchasingConfiguration()
        {
            ToTable("F_GoodsPurchasing");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

