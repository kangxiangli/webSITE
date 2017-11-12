
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PosLocationConfiguration : EntityConfigurationBase<PosLocation, int>
    {
        public PosLocationConfiguration()
        {
            ToTable("C_PosLocation");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

