using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class ArticleItemDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "父级栏目")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "栏目名称")]
        [Required, StringLength(15)]
        public virtual string ArticleItemName { get; set; }

        [Display(Name = "模版Id")]
        [Required]
        public virtual int TemplateId { get; set; }

        [Display(Name = "路径")]
        [Required, StringLength(30)]
        
        public virtual string ArticleItemPath { get; set; }

        [Display(Name = "Html路径")]
        [StringLength(100)]        
        public virtual string HtmlPath { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "是否在APP显示")]
        public virtual bool IsApp { get; set; }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }
    }
}
