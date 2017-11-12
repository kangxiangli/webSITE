
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
	[Serializable][Obsolete("已废弃")]
    public class MemberCard : EntityBase<int>
    {
        public MemberCard()
        {

        }

        [Display(Name = "储值卡名称")]
        public string CardName { get; set; }

        [Display(Name = "储值卡号")]
        public string CardNumber { get; set; }

        [Display(Name = "储值卡密")]
        public string CardKey { get; set; }

        [Display(Name = "储值卡简介")]
        public string Description { get; set; }

        [Display(Name = "卡内余额")]
        public decimal Balance { get; set; }

        [Display(Name = "起始时间")]
        public DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        public DateTime EndTime { get; set; }

        [Display(Name = "是否启用")]
        public bool IsUsed { get; set; }

        [Display(Name = "备注信息")]
        public string Notes { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


