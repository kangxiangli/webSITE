
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 问题答案
    /// </summary>
    public class NotificationAnswering : EntityBase<int>
    {
        [Display(Name = "问题标识符")]
        public virtual Guid QuestionGuidId { get; set; }

        [Display(Name = "唯一标识符")]
        public virtual Guid GuidId { get; set; }
        
        /// <summary>
        /// 答案(如果是判断题，内容为0或者1；0=×；1=√)
        /// </summary>
        [Display(Name = "答案(如果是判断题，内容为0或者1；0=×；1=√)")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 答案在本问题中的序号（字母为序号：a、b、c、d……）
        /// </summary>
        [Display(Name = "答案在本问题中的序号（字母为序号：a、b、c、d……）")]
        public virtual string Number { get; set; }

        [Display(Name = "是否为正确答案")]
        public virtual bool IsRight { get; set; }

        [Display(Name = "出题人信息")]
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
