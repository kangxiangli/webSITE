using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ArticleAttributeDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "父级栏目")]        
        public virtual int? ParentId { get; set; }

        [Display(Name = "属性名称")]
        [StringLength(15, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Required(ErrorMessage = "属性名称不能为空")]
        public virtual string AttributeName { get; set; }

        [Display(Name = "品牌编码")]
        [StringLength(5, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Required(ErrorMessage = "编码不能为空")]
        public virtual string ArticleAttrCode { get; set; }

        [Display(Name = "属性描述")]
        [MaxLength(120)]
        public virtual string Description { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
