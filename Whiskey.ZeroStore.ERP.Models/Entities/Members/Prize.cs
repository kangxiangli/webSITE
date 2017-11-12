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
    /// 奖品实体
    /// </summary>
    public class Prize : EntityBase<int>
    {
        [Display(Name="奖品名称")]
        [StringLength(20,ErrorMessage="最大长度不能超过{1}")]
        [Required(ErrorMessage="奖品名称不能为空")]
        public virtual string PrizeName { get; set; }

        [Display(Name = "奖品类型")]        
        public virtual int PrizeType { get; set; }

        [Display(Name = "奖品图片")]
        [StringLength(100)]
        public virtual string RewardImagePath { get; set; }

        [Display(Name = "积分")]
        public virtual int Score { get; set; }
        
        [Display(Name = "奖品数量")]
        public virtual int Quantity { get; set; }

        [Display(Name = "获取数量")]
        public virtual int GetQuantity { get; set; }

        [Display(Name = "领取数量")]
        public virtual int ReceiveQuantity { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
