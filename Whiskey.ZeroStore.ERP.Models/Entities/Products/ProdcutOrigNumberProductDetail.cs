using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 原始款号宝贝详情
    /// </summary>
    public class ProdcutOrigNumberProductDetail : EntityBase<int>
    {
        [Display(Name = "标记")]
        [StringLength(100)]
        public virtual string Mark { get; set; }

        [Display(Name = "内容HTML片段")]
        public virtual string Content { get; set; }

        [Display(Name = "内容")]
        public virtual string Text { get; set; }

        [Display(Name ="标记类型")]
        public TemplateTypeFlag TemplateTypeFlag { get; set; }
    }

    /// <summary>
    /// 模板类别
    /// </summary>
    [Serializable]
    public enum TemplateTypeFlag
    {
        PC = 0,
        手机 = 1
    }
}
