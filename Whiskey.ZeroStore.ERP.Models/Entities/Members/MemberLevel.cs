
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
    public class MemberLevel : EntityBase<int>
    {
        public MemberLevel()
        {
            Members = new List<Member>();
        }

        [Display(Name = "升级条件")]
        public virtual decimal UpgradeCondition { get; set; }

        [Display(Name = "升级方式")]
        public UpgradeType UpgradeType { get; set; }

        [Display(Name = "等级名称")]
        public virtual string LevelName { get; set; }

        [Display(Name = "等级折扣")]
        public virtual float Discount { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }

    /// <summary>
    /// 升级方式
    /// </summary>
    public enum UpgradeType
    {
        充值 = 0,
        企业 = 1
    }
}


