
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class WorkOrderCategoryDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "标识Id")]
        public int Id { get; set; }

        [Display(Name = "类别名称")]
        [StringLength(100, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string WorkOrderCategoryName { get; set; }

        [Display(Name = "备注")]
        public virtual string Notes { get; set; }
    }
}


