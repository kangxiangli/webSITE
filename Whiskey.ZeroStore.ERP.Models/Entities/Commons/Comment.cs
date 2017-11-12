using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 评论
    /// </summary>
    public class Comment : EntityBase<int>
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


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ReplyId")]
        public virtual Comment Reply { get; set; }
    }
}
