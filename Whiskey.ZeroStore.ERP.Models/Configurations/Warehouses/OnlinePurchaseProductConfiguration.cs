using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class OnlinePurchaseProductConfiguration : EntityConfigurationBase<OnlinePurchaseProduct, int>
    {
        public OnlinePurchaseProductConfiguration()
        {
            ToTable("W_Online_Purchase_Product");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
