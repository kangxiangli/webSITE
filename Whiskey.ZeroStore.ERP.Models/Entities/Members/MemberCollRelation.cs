using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员搭配师关系表
    /// </summary>
    public class MemberCollRelation : EntityBase<int>
    {
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "搭配师")]
        public virtual int CollocationId { get; set; }

        [Display(Name = "拉黑")]
        public virtual bool IsUnfriendly { get; set; }
    }
}
