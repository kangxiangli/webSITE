using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    /// <summary>
    /// 领域模型
    /// </summary>
    public class CommentDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "评论内容")]
        [StringLength(200)]
        public virtual string Content { get; set; }

        [Display(Name = ("评论对象Id"))]
        public virtual int SourceId { get; set; }

        [Display(Name = ("会员Id"))]
        public virtual int MemberId { get; set; }

        [Display(Name = ("回复评论Id"))]
        public virtual int? ReplyId { get; set; }  //0表示没有回复

        [Display(Name = ("评论来源"))]
        public virtual int CommentSource { get; set; }

        [Display(Name = ("标识Id"))]
        public Int32 Id { get; set; }
    }
}
