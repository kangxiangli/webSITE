
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AppointmentGenConfiguration : EntityConfigurationBase<AppointmentGen, int>
    {
        public AppointmentGenConfiguration()
        {
            ToTable("C_AppointmentGen");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}

