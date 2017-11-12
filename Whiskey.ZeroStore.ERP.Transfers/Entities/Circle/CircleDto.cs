using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class CircleDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "圈子名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{0}")]
        [Required(ErrorMessage = "请填写名称")]
        public virtual string CircleName { get; set; }

        [Display(Name = "图标")]
        [StringLength(120)]
        public virtual string IconPath { get; set; }

        [Display(Name = "备注")]
        [StringLength(120,ErrorMessage="最大不能超过{1}个字符")]
        public virtual string Notes { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
