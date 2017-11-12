
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class SizeExtentionConfiguration : EntityConfigurationBase<SizeExtention, int>
    {
        public SizeExtentionConfiguration()
        {
            ToTable("P_SizeExtention");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

