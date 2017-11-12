using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class BarCodeConfigConfiguration : EntityConfigurationBase<BarCodeConfig, int>
    {
        public BarCodeConfigConfiguration()
        {
            ToTable("P_BarCodeConfig");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
