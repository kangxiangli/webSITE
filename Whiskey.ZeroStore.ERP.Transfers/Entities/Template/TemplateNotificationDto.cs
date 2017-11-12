using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class TemplateNotificationDto : IAddDto, IEditDto<int>
    {

        [Display(Name = "标识Id")]
        public Int32 Id { get; set; }

        [Display(Name = "通知模板名称")]
        [Required, StringLength(15)]
        public virtual string Name { get; set; }

        [Display(Name = "模板标识说明")]
        //[StringLength(500)]
        public virtual string Notes { get; set; }

        [Display(Name = "通知分类")]
        [Required]
        public virtual TemplateNotificationType NotifciationType { get; set; }
    }
}
