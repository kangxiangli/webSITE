
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Module : EntityBase<int>
    {
        public Module() {
            Icon = "";
            ModuleName = "";
            ModuleType = 0;
            Description = "";
            PageUrl = "";
            PageArea = "";
            PageController = "";
            PageAction = "";
            Sequence++;
            onClickScripts = "";
            Children = new List<Module>();
            Permissions = new List<Permission>();
            IsShow = true;
        }


        [Display(Name = "父级模块")]
        [Index]
        public virtual int? ParentId { get; set; }

        [Display(Name = "模块名称")]
        [Required, StringLength(50)]
        public virtual string ModuleName { get; set; }

        [Display(Name = "模块图标")]
        public virtual string Icon { get; set; }

        [Display(Name = "模块类型")]
        public virtual int ModuleType { get; set; }

        [Display(Name = "模块简介")]
        public virtual string Description { get; set; }

        [Display(Name = "页面路径")]
        public virtual string PageUrl { get; set; }

        [Display(Name = "重写路径")]
        [StringLength(50,ErrorMessage="最大字符长度不能超过{1}")]
        public virtual string OverrideUrl { get; set; }

        [Display(Name = "页面区域")]
        public virtual string PageArea { get; set; }

        [Display(Name = "页面控制器")]
        public virtual string PageController { get; set; }

        [Display(Name = "页面方法")]
        public virtual string PageAction { get; set; }

        [Display(Name = "点击脚本")]
        public virtual string onClickScripts { get; set; }
        /// <summary>
        /// 该模块是否已经添加权限控制
        /// </summary>
        public virtual bool IsCompleteRule { get; set; }

        [Display(Name = "色值")]
        [StringLength(10)]
        public string ColorValue { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public virtual bool IsShow { get; set; }

        [Display(Name = "标识类名")]
        [StringLength(50)]
        public string BadgeTag { get; set; }

        [Display(Name = "标识路径")]
        [StringLength(300)]
        public string BadgeUrl { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


        [ForeignKey("ParentId")]
        public virtual Module Parent { get; set; }

        public virtual ICollection<Module> Children { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<Administrator> Adminis { get; set; }

        
    }
}


