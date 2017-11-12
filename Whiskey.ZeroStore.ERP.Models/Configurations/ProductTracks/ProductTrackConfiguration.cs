using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class ProductTrackConfiguration : EntityConfigurationBase<ProductTrack, int>
    {
        public ProductTrackConfiguration()
        {
            ToTable("P_ProductTrack");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);          
        }
    }
}
