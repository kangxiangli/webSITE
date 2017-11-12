
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class OrderFoodConfiguration : EntityConfigurationBase<OrderFood, int>
    {
        public OrderFoodConfiguration()
        {
            ToTable("O_OrderFood");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

