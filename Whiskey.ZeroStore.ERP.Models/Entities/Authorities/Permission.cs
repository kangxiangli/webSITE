
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
    public class Permission : EntityBase<int>
    {
        public Permission()
        {
            PermissionName = "";
            Icon = "";
            Style = "";
            Description = "";
            onClickScripts = "";
            IsDisplayIcon = false;
            Roles = new List<Role>();
        }

        [Display(Name = "所属模块")]
        public virtual int ModuleId { get; set; }

        [Display(Name = "操作名称")]
        [Required, StringLength(50)]
        public virtual string PermissionName { get; set; }

        [Display(Name = "操作简介")]
        public virtual string Description { get; set; }


        [Display(Name = "操作图标")]
        public virtual string Icon { get; set; }

        [Display(Name = "按钮样式")]
        public virtual string Style { get; set; }
        public virtual string Identifier { get; set; }
        /// <summary>
        /// 如果是以#开头表示id选择器，以.开头为class选择器，以小于号 &lt;开头表示这个标签的直接父类，大于号开头&gt;表示这个元素的直接子类，除此之外的都默认为mod-perm-defaul属性，当只存在#和.的时候可以构成复合选择
        /// </summary>
        [Display(Name = "唯一标识")]
        [StringLength(100)]
        public virtual string OnlyFlag { get; set; }
        [Display(Name = "操作请求所在域")]
        [StringLength(30)]
        public virtual string AreaName { get; set; }
        [Display(Name = "操作请求控制器")]
        [StringLength(30)]
        public virtual string ControllName { get; set; }
        [Display(Name = "操作请求方法")]
        [StringLength(30)]
        public virtual string ActionName { get; set; }

        [Display(Name = "权限逻辑分组")]
        public virtual PermissionGroupType? Gtype { get; set; }

        [Display(Name = "操作请求URL")]
        [StringLength(100)]
        public virtual string RequestUrl { get; set; }

        [Display(Name = "点击脚本")]
        public virtual string onClickScripts { get; set; }

        [Display(Name = "显示图标")]
        public virtual bool IsDisplayIcon { get; set; }

        [Display(Name = "显示文本")]
        public virtual bool IsDisplayText { get; set; }


        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
        //public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<Administrator> Adminis { get; set; }
        //public virtual ICollection<AAdministratorPermissionRelation> AAdministratorPermissionRelations { get; set; }
        public virtual ICollection<ARolePermissionRelation> ARolePermissionRelations { get; set; }
        //public virtual ICollection<AGroupPermissionRelation> AGroupPermissionRelations { get; set; }
        //public virtual ICollection<ADepartmentPermissionRelation> ADepartmentPermissionRelations { get; set; }
    }

    /// <summary>
    /// 权限表对应的分组
    /// </summary>
    [Serializable]
    public enum PermissionGroupType
    {
        查看 = 1,//加载页面、加载数据、打印、导出
        新增 = 2,
        删除 = 3,//移除数据、批量删除、恢复
        修改 = 4,
        禁用 = 5,//禁用数据、启用
    }
}


