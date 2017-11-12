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
    /// 点赞
    /// </summary>
    public class  Approval:EntityBase<int>
    {
        [Display(Name = ("评论来源"))]
        public virtual int SourceId { get; set; }

        [Display(Name = ("评论来源"))]
        public virtual int ApprovalSource { get; set; }

        //[Display(Name = ("来源类型"))]
        //public virtual int SourceType { get; set; }

        [Display(Name = ("会员Id"))]
        public virtual int MemberId { get; set; }

        [Display(Name = ("是否点赞"))]
        public virtual bool IsApproval { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
   
    }
}
