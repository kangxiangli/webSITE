
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class CollocationQuestionnaireDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "唯一标识")]
        public virtual Guid GuidId { get; set; }

        [Display(Name = "会员Id")]
        public virtual int MemberId { get; set; }

        [Display(Name = "问题名称")]
        public virtual string QuestionName { get; set; }

        [Display(Name = "回答内容")]
        public virtual string Content { get; set; }

        [ForeignKey("OperatorId")]
		public virtual Administrator Operator { get; set; }

        [ForeignKey("MemberId")]
		public virtual Member Member { get; set; }
    }
}


