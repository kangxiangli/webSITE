using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class MsgNotificationReader : EntityBase<int>
    {
        public int NotificationId { get; set; }
        public int? AdministratorId { get; set; }

        //[ForeignKey("NotificationId")]
        //public virtual Notification Notification { get; set; }

        [ForeignKey("AdministratorId")]
        public virtual Administrator Administrator { get; set; }

        public bool IsRead { get; set; }

        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; }
    }
}
