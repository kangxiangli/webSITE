using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class ArticleDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "文章标题")]
        [Required(ErrorMessage="请填写文章标题"), StringLength(50,ErrorMessage="不能超过{0}个字符")]
        public virtual string Title { get; set; }

        [Display(Name = "简介")]
        //[StringLength(150, ErrorMessage = "不能超过{0}个字符")]
        public virtual string Summary { get; set; }

        [Display(Name = "文件名")]        
        public virtual string FileName { get; set; }

        [Display(Name = "文章内容")]
        [Required(ErrorMessage = "请填写文章内容")]
        public virtual string Content { get; set; }

        [Display(Name = "文章路径")]        
        public virtual string ArticlePath { get; set; }

        [Display(Name = "封面图路径")]        
        public virtual string CoverImagePath { get; set; }

        [Display(Name = "跳转链接")]
        [StringLength(300,ErrorMessage="链接长度不能超过{1}")]
        public virtual string JumpLink { get; set; }

        [Display(Name = "置顶")]
        public virtual bool IsTop { get; set; }  //false表示否，true表示是

        [Display(Name = "热帖")]
        public virtual bool IsHot { get; set; }  //false表示否，true表示是

        [Display(Name = "推荐")]
        public virtual bool IsRecommend { get; set; }  //false表示否，true表示是

        [Display(Name = "展示图")]
        public virtual bool IsShow { get; set; }  //false表示否，true表示是

        [Display(Name = "点击次数")]
        public virtual int Hits { get; set; }

        [Display(Name = "审核状态")]
        public virtual int VerifyType { get; set; }

        [Display(Name = "模版")]
        public virtual int? TemplateId { get; set; }

        [Display(Name = "栏目")]
        [Required(ErrorMessage="请选择文章栏目")]
        public virtual int ArticleItemId { get; set; }

        [Required(ErrorMessage = "一级栏目")]
        public virtual int ParentArticleItemId { get; set; }

        public virtual int? AdminId { get; set; }
        public virtual DateTime CreateTime { get; set; }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

    }
}
