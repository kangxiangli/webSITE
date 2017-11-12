using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MsgNotificationConfiguration : EntityConfigurationBase<MsgNotificationReader, int>
    {
        public MsgNotificationConfiguration()
        {
            ToTable("N_MsgNotificationReader");
			Property(m => m.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
