
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class NotificationAnsweringConfiguration : EntityConfigurationBase<NotificationAnswering, int>
    {
        public NotificationAnsweringConfiguration()
        {
            ToTable("N_Notification_Answering");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
