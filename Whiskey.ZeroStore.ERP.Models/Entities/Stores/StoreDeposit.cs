using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreDeposit : EntityBase<int>
    {
        [Display(Name = "充值店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "现金")]
        public virtual float Cash { get; set; }

        [Display(Name = "刷卡")]
        public virtual float Card { get; set; }

        [Display(Name = "汇款")]
        public virtual float Remit { get; set; }

        [Display(Name = "总计")]
        public virtual float Price { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "付款类型")]
        public virtual StoreDepositTypeFlag DepositType { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        public virtual Administrator Operator { get; set; }
    }
}
