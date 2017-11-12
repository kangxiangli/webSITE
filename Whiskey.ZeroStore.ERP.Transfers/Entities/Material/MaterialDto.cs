using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class MaterialDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "素材名称")]
        [Required, StringLength(8)]
        public string MaterialName { get; set; }

        //[Display(Name = "素材编码")]
        //[StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        //[Required(ErrorMessage = "编码不能为空！")]
        //[RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        //public string MaterialCode { get; set; }

        [Display(Name = "素材类型")]
        public int MaterialType { get; set; }

        [Display(Name = "素材图标")]
        [StringLength(100)]
        public string IconPath { get; set; }

        [Display(Name = "标识")]
        public Int32 Id { get; set; }
    }
}
