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
    public class TopicImage: EntityBase<int>
    {        

        //[Display(Name = "话题")]
        //public virtual int TopicId { get; set; }

        [Display(Name = "图片路径")]
        [StringLength(100, ErrorMessage = "最大长度不能超过{1}")]        
        public virtual string ImagePath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        //[ForeignKey("TopicId")]
        //public virtual Topic Topic { get; set; }
    }
}
