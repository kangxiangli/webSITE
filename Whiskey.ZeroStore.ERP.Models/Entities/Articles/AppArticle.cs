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
    public class AppArticle : EntityBase<int>
    {

        [Display(Name = ("会员"))]
        public virtual int MemberId { get; set; }

        [Display(Name = ("文章标题"))]        
        [StringLength(25)]        
        public virtual string ArticleTitle { get; set; }

        [Display(Name = ("封面图片"))]
        [StringLength(100)]
        public virtual string CoverImagePath { get; set; }

        [Display(Name = ("是否推荐"))]
        public virtual bool IsRecommend { get; set; }

        [Display(Name = ("是否有效果"))]
        public virtual bool IsEffect { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        public string DeviceType { get; set; }

        public virtual ICollection<AppArticleItem> AppArticleItems { get; set; }
    }
}
