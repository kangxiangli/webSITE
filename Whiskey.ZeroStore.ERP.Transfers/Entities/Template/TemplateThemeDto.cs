using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class TemplateThemeDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "标识Id")]
        public Int32 Id { get; set; }

        [Display(Name = "主题名称")]
        [Required, StringLength(15)]
        public virtual string Name { get; set; }

        [Display(Name = "主题说明")]
        [StringLength(150)]
        public virtual string Notes { get; set; }

        [Display(Name = "所在路径")]
        [StringLength(150)]
        public virtual string Path { get; set; }

        [Display(Name = "默认主题")]
        public virtual bool IsDefault { get; set; }

        [Display(Name = "主题Logo")]
        public virtual string ThemeLogo { get; set; }

        [Display(Name = "登录背景图")]
        public virtual string BackgroundImg { get; set; }

        [Display(Name = "主题类型")]
        public virtual TemplateThemeFlag ThemeFlag { get; set; }
    }
}
