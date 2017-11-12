using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    
    public class HtmlPart : EntityBase<int>
    {
        [Display(Name = "名称")]
        [Required, StringLength(50, ErrorMessage = "名称在{0}个字符以内")]
        public virtual string PartName { get; set; }

        [Display(Name = "内容")]
        [StringLength(2000, ErrorMessage = ("简介在{0}个字符以内"))]
        public virtual string Content { get; set; }

        [Display(Name = "简介")]
        [StringLength(120, ErrorMessage = ("简介在{0}个字符以内"))]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
