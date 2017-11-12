
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberRoleDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "角色名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Name { get; set; }

        [Display(Name = "描述")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Description { get; set; }

        [Display(Name = "权重")]
        public virtual int Weight { get; set; }
    }
}


