using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class MsgNotificationReaderDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "标识Id")]
        public virtual int Id { get; set; }

        public int NotificationId { get; set; }
        public int? AdministratorId { get; set; }

        //[ForeignKey("NotificationId")]
        //public virtual Notification Notification { get; set; }

        [ForeignKey("AdministratorId")]
        public virtual Administrator Administrator { get; set; }

        public bool IsRead { get; set; }
    }
}
