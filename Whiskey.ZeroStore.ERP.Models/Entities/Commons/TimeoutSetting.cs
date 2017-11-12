



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
    public class TimeoutSetting : EntityBase<int>
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Display(Name = "名称")]
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Display(Name = "描述")]
        public string Desc { get; set; }

        /// <summary>
        /// 超时秒数
        /// </summary>
        [Display(Name = "超时时间(单位:秒)")]

        public int TimeSpan { get; set; }

        /// <summary>
        /// 扣除积分
        /// </summary>
        [Display(Name = "扣除积分")]
        public int DeductScore {
            get;set;
        }


        /// <summary>
        /// 操作人
        /// </summary>
        [Display(Name = "操作人")]
        [ForeignKey("OperatorId")]
        public Administrator Operator { get; set; }
    }
}


