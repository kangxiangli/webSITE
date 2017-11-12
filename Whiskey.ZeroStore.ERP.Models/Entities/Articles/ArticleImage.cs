using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    [Serializable]
    public class ArticleImage : EntityBase<int>
    {
        [Display(Name = "图片名称")]
        [Required, MaxLength(200)]
        public virtual string ImageName { get; set; }
        [Display(Name = "发布人")]
        [Required, StringLength(10)]
        public virtual string ImagePublisher { get; set; }

        [Display(Name = "图片路径")]
        [Required, StringLength(300)]
        public virtual string ImagePath { get; set; }
        [Display(Name="图片类型")]
        [Required]
        public virtual int ImageStatus { get; set; }//0表示封面图，1表示文章图
    }
}
