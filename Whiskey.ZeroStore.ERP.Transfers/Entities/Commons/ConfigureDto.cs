
using System;
using Whiskey.Core.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ConfigureDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

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

        public virtual int? OperatorId { get; set; }
    }
}


