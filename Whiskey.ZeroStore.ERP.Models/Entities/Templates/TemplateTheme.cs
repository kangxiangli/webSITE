using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class TemplateTheme : EntityBase<int>
    {
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

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
