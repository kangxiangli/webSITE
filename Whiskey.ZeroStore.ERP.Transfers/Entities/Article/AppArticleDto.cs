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
    public class AppArticleDto : IAddDto, IEditDto<int>
    {
        public AppArticleDto()
        {
            AppArticleItems = new List<AppArticleItem>();
        }

        [Display(Name = ("会员"))]
        public virtual int MemberId { get; set; }

        [Display(Name = ("文章标题"))]        
        [StringLength(25)]
        public virtual string ArticleTitle { get; set; }

        [Display(Name = ("封面图片"))]
        [StringLength(100)]
        public virtual string CoverImagePath { get; set; }

        [Display(Name = ("推荐"))]
        public virtual bool IsRecommend { get; set; }

        [Display(Name = ("标识Id"))]
        public virtual Int32 Id { get; set; }

        [Display(Name = ("是否有效果"))]
        public virtual bool IsEffect { get; set; }

        public string DeviceType { get; set; }

        public virtual ICollection<AppArticleItem> AppArticleItems { get; set; }
    }
}
