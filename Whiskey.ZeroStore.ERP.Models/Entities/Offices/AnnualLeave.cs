using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AnnualLeave: EntityBase<int>
    {       

        [DisplayName("年假名称")]
        [Required(ErrorMessage="年假名称不能为空")]
        [StringLength(10, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string AnnualLeaveName { get; set; }

        [DisplayName("父级")]
        public virtual int? ParentId { get; set; }

        //[DisplayName("年假类型")]
        //public virtual int AnnualLeaveType { get; set; }

        [DisplayName("工作年限")]
        [Required(ErrorMessage = "工作年限不能为空")]
        public virtual int  StartYear { get; set; }

        [DisplayName("工作年限")]
        [Required(ErrorMessage = "工作年限不能为空")]
        public virtual int EndYear { get; set; }

        [DisplayName("年假天数")]
        [Required(ErrorMessage = "年假天数不能为空")]
        public virtual int Days { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string Notes { get; set; }

        [ForeignKey("ParentId")]
        public virtual AnnualLeave Parent { get; set; }

        public virtual ICollection<AnnualLeave> Children { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
         
    }
}
