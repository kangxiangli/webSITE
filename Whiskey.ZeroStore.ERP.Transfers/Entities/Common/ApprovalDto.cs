using System;
using System.Collections.Generic;
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
    public class ApprovalDto : IAddDto, IEditDto<int>
    {
        [Display(Name = ("评论来源"))]
        public virtual int SourceId { get; set; }

        [Display(Name = ("评论来源"))]
        public virtual int ApprovalSource { get; set; }

        //[Display(Name = ("商品类型"))]
        //public virtual int ProductType { get; set; }

        [Display(Name = ("会员Id"))]
        public virtual int MemberId { get; set; }

        [Display(Name = ("是否点赞"))]
        public virtual bool IsApproval { get; set; }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }
    }
}
