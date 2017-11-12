
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class MemberShare : EntityBase<int>
    {
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [Display(Name = "来源")]
        public virtual ShareFlag Flag { get; set; }

        [Display(Name = "游戏来源")]
        public virtual int? GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }
    }
}

