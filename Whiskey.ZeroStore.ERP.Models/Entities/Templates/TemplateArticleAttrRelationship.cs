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
    /// 模版文章栏目关系表
    /// </summary>
    public class TemplateArticleAttrRelationship : EntityBase<int>
    {
        [Display(Name = ("文章栏目Id"))]
        public virtual int ArticleAttrId { get; set; }

        [Display(Name=("模版Id"))]
        public virtual int TemplateId { get; set; }

        [Display(Name = "父级Id")]
        public virtual int? ParentId { get; set; }

        [Display(Name = ("文章栏目路径"))] 
        [StringLength(300)]
        public virtual string ArticleAttrPath { get; set; }

        [ForeignKey("ParentId")]
        public virtual TemplateArticleAttrRelationship Parent { get; set; }

        public virtual ICollection<TemplateArticleAttrRelationship> Children { get; set; }
    }
}
