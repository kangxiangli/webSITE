
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class WorkOrderCategory : EntityBase<int>
    {
        [Display(Name = "类别名称")]
        [StringLength(100, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string WorkOrderCategoryName { get; set; }

        [Display(Name = "备注")]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

