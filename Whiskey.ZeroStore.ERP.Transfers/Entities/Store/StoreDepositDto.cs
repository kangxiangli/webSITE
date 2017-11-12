using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class StoreDepositDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "充值店铺")]
        public virtual Int32 StoreId { get; set; }

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
        public virtual String Notes { get; set; }

        [Display(Name = "付款类型")]
        public virtual StoreDepositTypeFlag DepositType { get; set; }
    }
}
