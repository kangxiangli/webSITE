
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 问题
    /// </summary>
    public class NotificationQuestion : EntityBase<int>
    {
        [Display(Name = "唯一标识符")]
        public virtual Guid GuidId { get; set; }

        [Display(Name = "问题对应的消息Id")]
        public virtual int NotificationId { get; set; }

        [Display(Name = "问题内容")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 问题类型（0:选择题；1：填空题；2：判断题）
        /// </summary>
        [Display(Name = "问题类型（0:选择题；1：填空题；2：判断题）")]
        public virtual int QuestionType { get; set; }

        /// <summary>
        /// 回答过该问题的用户的数量
        /// </summary>
        [Display(Name = "回答过该问题的用户的数量")]
        [DefaultValue(0)]
        public virtual int AnswerersCount { get; set; }

        [Display(Name = "该问题的答案")]
        public virtual List<NotificationAnswering> AnsweringsList { get; set; }

        [Display(Name = "回答该问题的人的答题信息")]
        public virtual List<NotificationAnswerers> AnswerersList { get; set; }

        [Display(Name = "出题人信息")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

