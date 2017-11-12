
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class CollocationQuestionnaire : EntityBase<int>
    {
        [Display(Name = "唯一标识")]
        public virtual Guid GuidId { get; set; }

        [Display(Name = "会员Id")]
        public virtual int MemberId { get; set; }

        [Display(Name = "问题名称")]
        public virtual string QuestionName { get; set; }

        [Display(Name = "回答内容")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 评分,详细测试报告结果用
        /// </summary>
        [Display(Name = "评分")]
        public virtual int Score { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

    }
}

