
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class NotificationQuestionDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "问题对应的消息Id")]
        public virtual int NotificationId { get; set; }

        [Display(Name = "唯一标识符")]
        public virtual Guid GuidId { get; set; }

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
        public virtual int AnswerersCount { get; set; }

        [Display(Name = "操作人（即出题人）")]
        public virtual int? OperatorId { get; set; }

        [Display(Name = "出题人信息")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}


