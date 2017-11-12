
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class MemberModule : EntityBase<int>
	{
        public MemberModule()
        {
            IsShow = true;
            Sequence++;
            Children = new List<MemberModule>();
        }

        [Display(Name = "父级模块")]
        [Index]
        public virtual int? ParentId { get; set; }

        [Display(Name = "模块名称")]
        [Required, StringLength(50)]
        public virtual string ModuleName { get; set; }

        [Display(Name = "模块图标")]
        public virtual string Icon { get; set; }

        [Display(Name = "模块简介")]
        public virtual string Description { get; set; }

        [Display(Name = "页面路径")]
        public virtual string PageUrl { get; set; }

        [Display(Name = "重写路径")]
        [StringLength(50, ErrorMessage = "最大字符长度不能超过{1}")]
        public virtual string OverrideUrl { get; set; }

        [Display(Name = "页面区域")]
        public virtual string PageArea { get; set; }

        [Display(Name = "页面控制器")]
        public virtual string PageController { get; set; }

        [Display(Name = "页面方法")]
        public virtual string PageAction { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        [Display(Name = "是否显示")]
        public virtual bool IsShow { get; set; }

        [ForeignKey("ParentId")]
        public virtual MemberModule Parent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<MemberModule> Children { get; set; }

        public virtual ICollection<MemberRole> MemberRoles { get; set; }
    }
}

