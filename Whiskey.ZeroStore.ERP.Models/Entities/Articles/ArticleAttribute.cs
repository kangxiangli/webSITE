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
    [Serializable]
    public class ArticleAttribute :EntityBase<int>
    {
        /// <summary>
        /// 构造函数--初始化对象
        /// </summary>
        public ArticleAttribute()
        {
            Articles = new List<Article>();
            Children = new List<ArticleAttribute>();
        }
        [Display(Name = "父级栏目")]        
        [Index]
        public virtual int? ParentId { get; set; }

        [Display(Name = "属性名称")]
        [StringLength(15, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Required(ErrorMessage = "属性名称不能为空")]
        public virtual string AttributeName { get; set; }

        [Display(Name = "品牌编码")]
        [StringLength(5, ErrorMessage = "最大长度不能超过{1}个字符")]
        //[Required(ErrorMessage = "编码不能为空")]
        public virtual string ArticleAttrCode { get; set; }

        [Display(Name = "属性描述")]
        [MaxLength(120)]
        public virtual string Description { get; set; }     

        [ForeignKey("OperatorId")]
        [Index]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 层级关系-父级对象
        /// </summary>
        [ForeignKey("ParentId")]
        [Index]
        public virtual ArticleAttribute Parent { get; set; }
        
        public virtual ICollection<Article> Articles { get; set; }
        
        public virtual ICollection<ArticleAttribute> Children { get; set; }
    }
}
