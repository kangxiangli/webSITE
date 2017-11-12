
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Notification : EntityBase<int>
    {
        public Notification() {
            PushNotifications = new List<MsgNotificationReader>();
        }
			
		[Display(Name = "通知标题")]
        [Required(ErrorMessage="请填写标题")]
        [StringLength(50,ErrorMessage="最大长度不能超过{1}个字符")]
        public virtual string Title { get; set; }

        [Display(Name = "通知内容")]
        [Required(ErrorMessage = "请填写内容")]
        //[StringLength(500, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Description { get; set; }

        [Display(Name = "是否启用App")]
        public virtual bool IsEnableApp { get; set; }

        [Display(Name = "通知类型")]
        public virtual int NoticeType { get; set; }

        [Display(Name = "发送时间")]
        public virtual DateTime? SendTime { get; set; }

        [Display(Name = "通知目标")]
        public virtual int NoticeTargetType { get; set; }

        [Display(Name = "是否成功")]
        public virtual bool IsSuccessed { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<MsgNotificationReader> PushNotifications { get; set; }

    }
}


