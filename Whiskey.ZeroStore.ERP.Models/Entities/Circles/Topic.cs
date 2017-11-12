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
    public class Topic: EntityBase<int>
    {
        public Topic()
        {
            TopicImages = new List<TopicImage>();
        }


        [Display(Name = "话题名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}")]
        [Required(ErrorMessage = "请填写名称")]
        public virtual string TopicName { get; set; }

        [Display(Name = "圈子")]
        public virtual int CircleId { get; set; }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "话题内容")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{1}")]        
        public virtual string Content { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("CircleId")]
        public virtual Circle Circle { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        public virtual ICollection<TopicImage> TopicImages { get; set; }
    }
}
