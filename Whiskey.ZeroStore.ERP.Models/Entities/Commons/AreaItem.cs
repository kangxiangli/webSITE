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
    public class AreaItem : EntityBase<int>
    {
        [Display(Name = "区域名称")]
        [Required, StringLength(50)]
        public virtual string AreaName { get; set; }

        [Display(Name = "父级")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "父级编码")]        
        public virtual int? ParentNum { get; set; }

        [Display(Name = "区域编码")]
        [Required]
        public virtual int AreaNum { get; set; }         

        [Display(Name = "邮政编码")]
        public virtual int? ZipCode { get; set; }

        [Display(Name = "编码")]
        public virtual int? AreaCode { get; set; }

        [ForeignKey("ParentId")]
        public virtual AreaItem Parent { get; set; }               

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<AreaItem> Children { get; set; }
    }
}
