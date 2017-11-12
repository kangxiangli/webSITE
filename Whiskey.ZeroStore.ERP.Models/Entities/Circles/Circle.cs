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
    /// <summary>
    /// 圈子
    /// </summary>
    public class Circle : EntityBase<int>
    {
        public Circle()
        {
            Topics = new List<Topic>();
            Members = new List<Member>();
        }

        [Display(Name = "圈子名称")]
        [StringLength(20,ErrorMessage="最大长度不能超过{1}")]
        [Required(ErrorMessage="请填写名称")]
        public virtual string CircleName { get; set; }

        [Display(Name = "图标")]
        [StringLength(120)]
        public virtual string IconPath { get; set; }

        [Display(Name = "备注")]
        [StringLength(120, ErrorMessage = "最大不能超过{1}个字符")]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Topic> Topics { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
