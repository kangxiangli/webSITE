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
    /// <summary>
    /// 文章实体模型
    /// </summary>
    [Serializable]
    public class Article:EntityBase<int>
    {
        /// <summary>
        /// 构造函数-初始化Article对象
        /// </summary>
        public Article()
        {
            IsTop = false;
            IsHot = false;
            IsRecommend = false;           
            Hits = 0;
            JumpLink = string.Empty;            
        }
        [Display(Name = "文章标题")]
        [Required, StringLength(50)]
        public virtual string Title { get; set; }

        [Display(Name="简介")]
        //[StringLength(150)]
        public virtual string Summary { get; set; }

        [Display(Name = "文件名")]
        [StringLength(40)]
        public virtual string FileName { get; set; }

        [Display(Name = "文章内容")]
        [Required]
        public virtual string Content { get; set; }

        [Display(Name="文章路径")]
        [StringLength(200)]
        public virtual string ArticlePath { get; set; }

        [Display(Name = "封面图路径")]
        [StringLength(150)]
        public virtual string CoverImagePath { get; set; }

        [Display(Name = "跳转链接")]
        [StringLength(300, ErrorMessage = "链接长度不能超过{1}")]
        public virtual string JumpLink { get; set; }

        [Display(Name = "是否置顶")]
        public virtual bool IsTop { get; set; }  //false表示否，true表示是

        [Display(Name = "是否热帖")]
        public virtual bool IsHot { get; set; }  //false表示否，true表示是

        [Display(Name = "是否推荐")]
        public virtual bool IsRecommend { get; set; }  //false表示否，true表示是

        [Display(Name = "是否设为展示图")]
        public virtual bool IsShow { get; set; }  //false表示否，true表示是

        [Display(Name = "点击次数")]
        public virtual int Hits { get; set; }

        [Display(Name = "审核状态")]        
        public virtual int VerifyType { get; set; } 

        [Display(Name = "模版ID")]
        public virtual int? TemplateId { get; set; }

        [Display(Name = "模版ID")]
        public virtual int? AdminId { get; set; }

        [Display(Name = "栏目Id")]
        public virtual int ArticleItemId { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("ArticleItemId")]
        public virtual ArticleItem ArticleItem { get; set; }
    }
}
