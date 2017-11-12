
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
    public class Log : EntityBase<int>
    {
        public Log() {

        }
		

		[Display(Name = "日志标题")]
        public virtual string LogName { get; set; }

        [Display(Name = "日志内容")]
        public virtual string Description { get; set; }

        [Display(Name = "页面地址")]
        public virtual string PageUrl { get; set; }

        [Display(Name = "IP地址")]
        public virtual string IPAddress { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


