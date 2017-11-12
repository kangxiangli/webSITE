using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class WorkLogAttributeDto : IAddDto, IEditDto<int>
    {
        [DisplayName("项目名称")]
        [StringLength(10, ErrorMessage = "最大长度不能超过{1}")]
        [Required(ErrorMessage = "请填写")]
        public virtual string WorkLogAttributeName { get; set; }

        [DisplayName("父级")]
        public virtual int? ParentId { get; set; }

        [DisplayName("备注")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}")]
        public virtual string Notes { get; set; }

        [DisplayName("标识Id")]
        public virtual int Id { get; set; }

        [DisplayName("该栏目下的手册数量")]
        [DefaultValue(0)]
        public virtual int WorkLogCount { get; set; }
    }
}
