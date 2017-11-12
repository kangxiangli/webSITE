



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
    public class Messager : EntityBase<int>
    {
        public Messager() {
            //Receiver = new Administrator();
        }

        [Display(Name = "发送人")]
        public virtual int? SenderId { get; set; }

        [Display(Name = "接收人")]
        public virtual int? ReceiverId { get; set; }

        [Display(Name = "消息标题")]
        [StringLength(100,ErrorMessage="消息标题不能超过{1}个字符")]
        [Required(ErrorMessage="请填写消息标题")]
        public virtual string MessageTitle { get; set; }

        [Display(Name = "消息类型")]
        public virtual int MessageType { get; set; }

        [Display(Name = "消息内容")]
        [StringLength(500,ErrorMessage="消息内容不能超过{1}个字符")]
        [Required(ErrorMessage="请填写消息内容")]
        public virtual string Description { get; set; }

        [Display(Name = "消息状态")]
        public virtual int Status { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual Administrator Receiver { get; set; }

        [ForeignKey("SenderId")]
        public virtual Administrator Sender { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


