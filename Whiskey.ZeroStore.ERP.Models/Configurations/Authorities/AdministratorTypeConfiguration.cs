using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AdministratorTypeConfiguration : EntityConfigurationBase<AdministratorType, int>
    {
        public AdministratorTypeConfiguration()
        {
            ToTable("A_Administrator_Type");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
