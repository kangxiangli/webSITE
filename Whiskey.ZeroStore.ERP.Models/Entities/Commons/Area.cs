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
    public class Area : EntityBase<int>
    {
        [Display(Name = "区域名称")]
        [Required, StringLength(20)]
        public virtual string AreaName { get; set; }

        [Display(Name = "区域编码")]
        [Required]
        public virtual int AreaNum { get; set; }

        [Display(Name = "父级名称")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "邮政编码")]
        public virtual int ZipCode { get; set; }

        [ForeignKey("ParentId")]
        public virtual Area Parent { get; set; }

        public virtual ICollection<Area> Child { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
