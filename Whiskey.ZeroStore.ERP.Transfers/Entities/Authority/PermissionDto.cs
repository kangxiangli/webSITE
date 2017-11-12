



using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PermissionDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
		[Display(Name = "模块ID")]
		public Int32 ModuleId { get; set; }

		[Display(Name = "操作名称")]
		[Required(ErrorMessage = "不能为空")]
		[StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
		public String PermissionName { get; set; }

		[Display(Name = "操作简介")]
		public String Description { get; set; }


        [Display(Name = "操作图标")]
        public virtual string Icon { get; set; }

        [Display(Name = "按钮样式")]
        public virtual string Style { get; set; }
        [Display(Name = "识别名称")]
        public virtual string Identifier { get; set; }
        /// <summary>
        /// 如果是以#开头表示id选择器，以.开头为class选择器，以小于号 &lt;开头表示这个标签的直接父类，大于号开头&gt;表示这个元素的直接子类，除此之外的都默认为mod-perm-defaul属性，当只存在#和.的时候可以构成复合选择
        /// </summary>
        [Display(Name = "对应页面唯一标识")]
        [StringLength(100)]
        public virtual string OnlyFlag { get; set; }
        [Display(Name = "域")]
        [StringLength(20)]
        public virtual string AreaName { get; set; }
        [Display(Name = "控制器名")]
        [StringLength(20)]
        public virtual string ControllName { get; set; }
        [Display(Name = "访问方法名")]
        [StringLength(20)]
        public virtual string ActionName { get; set; }

        [Display(Name = "权限逻辑分组")]
        public virtual PermissionGroupType? Gtype { get; set; }

        [Display(Name = "请求相对路径")]
        public virtual string RequestUrl { get; set; }

        [Display(Name = "点击脚本")]
        public virtual string onClickScripts { get; set; }

        [Display(Name = "显示图标")]
        public virtual bool IsDisplayIcon { get; set; }

        [Display(Name = "显示文本")]
        public virtual bool IsDisplayText { get; set; }

    }
}
