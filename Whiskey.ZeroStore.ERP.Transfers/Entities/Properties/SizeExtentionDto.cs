
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class SizeExtentionDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Name { get; set; }
    }
}


