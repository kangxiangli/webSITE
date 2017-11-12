using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class TemplateDto: IAddDto, IEditDto<int>
    {
        public TemplateDto()
        {
            EnabledPerNotifi = true;
        }

        [Display(Name = "模板名称")]
        [Required, StringLength(15)]
        public virtual string TemplateName { get; set; }

        [Display(Name = "静态页名称")]
        public virtual string HtmlName { get; set; }

        [Display(Name = "模版分类")]
        [Required]
        public virtual int TemplateType { get; set; }

        [Display(Name = "父级Id")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "模块通知Id")]
        public virtual int? TemplateNotificationId { get; set; }

        [Display(Name = "模板路径")]
        [StringLength(100)]
        public virtual string TemplatePath { get; set; }

        [Display(Name = "模版内容")]
        [Required(ErrorMessage="请填写")]
        public virtual string TemplateHtml { get; set; }

        [Display(Name = "备注")]
        [StringLength(150)]
        public virtual string Notes { get; set; }        

        [Display(Name = "是否为默认")]
        public virtual bool IsDefault { get; set; }

        [Display(Name = "是否为手机默认")]
        public virtual bool IsDefaultPhone { get; set; }

        [Display(Name="标识Id")]
        public Int32 Id { get; set; }

        [Display(Name = "模块通知标识")]
        public virtual TemplateNotification templateNotification { get; set; }

        /// <summary>
        /// 启用权限通知,false时根据部门类型通知
        /// </summary>
        [Display(Name = "权限通知")]
        public virtual bool EnabledPerNotifi { get; set; }

        /// <summary>
        /// 权限通知到的部门类型,以”,“分隔
        /// </summary>
        [Display(Name = "部门类型")]
        public virtual string DepartTypeFlags { get; set; }
    }
     
}
