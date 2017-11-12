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
    public class MissionComment: EntityBase<int>
    {
        [Display(Name = "评论级别")]
        public virtual int StarLevel { get; set; } //1-5个级别

        [Display(Name = "评论内容")]
        [StringLength(200)]
        public virtual string Content { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
