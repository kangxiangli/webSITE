using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class TopicDto : IAddDto, IEditDto<int>
    {
        public TopicDto()
        {
            TopicImages = new List<TopicImage>();
        }

        [Display(Name = "话题名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{0}")]
        [Required(ErrorMessage = "请填写名称")]
        public virtual string TopicName { get; set; }

        [Display(Name = "圈子")]
        public virtual int CircleId { get; set; }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "话题内容")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{0}")]
        public virtual string Content { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }

        public virtual ICollection<TopicImage> TopicImages { get; set; }
    }
}
