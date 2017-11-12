using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class Configure : EntityBase<int>
    {
        /// <summary>
        /// 配置文件名
        /// </summary>
        [Display(Name = "配置文件名")]
        public virtual string DirName { get; set; }

        /// <summary>
        /// 配置类名
        /// </summary>
        [Display(Name = "配置类名")]
        public virtual string ClassName { get; set; }

        /// <summary>
        /// 配置节点名称
        /// </summary>
        [Display(Name = "配置节点名称")]
        public virtual string NodeName { get; set; }

        /// <summary>
        /// 配置值
        /// </summary>
        [Display(Name = "配置值")]
        public virtual string Value { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Display(Name = "备注")]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

