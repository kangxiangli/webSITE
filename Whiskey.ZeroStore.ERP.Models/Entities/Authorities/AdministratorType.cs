using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AdministratorType : EntityBase<int>
    {
        [Display(Name = "类型名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string TypeName { get; set; }

        [Display(Name = "不可修改")]
        public virtual bool UnChangeable { get; set; }

        [Display(Name = "备注")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Notes { get; set; }

    }
}
