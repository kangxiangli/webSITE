using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AppArticleItemDto : IAddDto, IEditDto<int>
    {
        [Display(Name = ("手机文章"))]
        public virtual int AppArticleId { get; set; }

        [Display(Name = ("图片素材"))]
        public virtual int? GalleryId { get; set; }

        [Display(Name = ("文章素材类型"))]
        public virtual int AppArticleItemType { get; set; }

        [Display(Name = ("位置"))]
        [StringLength(100)]
        public virtual string Position { get; set; }

        [Display(Name = ("比列"))]
        [StringLength(50)]
        public virtual string Ratio { get; set; }

        [Display(Name = ("图片路径"))]
        [StringLength(100)]
        public virtual string ImagePath { get; set; }

        [Display(Name = ("旋转"))]
        [StringLength(100)]
        public virtual string Rotation { get; set; }

        [Display(Name = ("对齐样式"))]
        [StringLength(2)]
        public virtual string AlignStyle { get; set; }

        [Display(Name = ("文字内容"))]
        [StringLength(200)]
        public virtual string Content { get; set; }

        [Display(Name = ("文字颜色"))]
        [StringLength(100)]
        public virtual string FontColor { get; set; }

        [Display(Name = ("文字大小"))]
        [StringLength(15)]
        public virtual string FontSize { get; set; }

        [Display(Name = ("文本行数"))]
        public virtual int ContentRow { get; set; }

        [Display(Name = ("模板类型"))]
        public virtual int TemplateType { get; set; }

        [Display(Name = ("标识Id"))]
        public virtual Int32 Id { get; set; }
    }
}
