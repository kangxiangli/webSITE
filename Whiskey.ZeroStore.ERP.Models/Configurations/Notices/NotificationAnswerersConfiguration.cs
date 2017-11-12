
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    class NotificationAnswerersConfiguration : EntityConfigurationBase<NotificationAnswerers, int>
    {
        public NotificationAnswerersConfiguration()
        {
            ToTable("N_Notification_Answerers");
            Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
