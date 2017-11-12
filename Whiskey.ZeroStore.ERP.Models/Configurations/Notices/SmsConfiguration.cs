using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class SmsConfiguration : EntityConfigurationBase<Sms, int>
    {
        public SmsConfiguration()
        {
            ToTable("N_Sms");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
