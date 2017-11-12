
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 答题者答题信息
    /// </summary>
    public class NotificationAnswerers : EntityBase<int>
    {
        [Display(Name = "回答的问题对应的消息Id")]
        public virtual int NotificationId { get; set; }

        [Display(Name = "回答的问题对应的用户消息Id")]
        public virtual int MsgNotificationId { get; set; }

        [Display(Name = "问题标识符")]
        public virtual Guid QuestionGuidId { get; set; }

        [Display(Name = "唯一标识符")]
        public virtual Guid GuidId { get; set; }

        /// <summary>
        /// 回答的内容
        /// <value>选择题 :选择的答案Id</value>
        /// <value>填空题：用户填写的内容</value>
        /// <value>判断题：0或者1（0是×；1是√）</value>
        /// </summary>
        [Display(Name = "回答的内容（选择题 :选择的答案Id; 填空题：用户填写的内容；判断题：0或者1（0是×；1是√））")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 问题类型（0:选择题；1：填空题；2：判断题）
        /// </summary>
        [Display(Name = "问题类型（0:选择题；1：填空题；2：判断题）")]
        public virtual int QuestionType { get; set; }

        [Display(Name = "是否正确")]
        public virtual bool IsRight { get; set; }

        [Display(Name = "答题人Id")]
        public virtual int AdministratorId { get; set; }

        [Display(Name = "答题人信息")]
        [ForeignKey("AdministratorId")]
        public virtual Administrator AdministratorInfo { get; set; }

        [Display(Name = "操作人信息")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}
