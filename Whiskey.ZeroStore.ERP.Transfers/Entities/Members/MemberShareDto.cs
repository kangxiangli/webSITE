
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberShareDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "来源")]
        public virtual ShareFlag Flag { get; set; }
    }
}


