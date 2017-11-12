using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
  public   class RechargeOrder : EntityBase<int>
    {
        /// <summary>
        ///用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 100001-999999 六位数编号
        /// </summary>
        [Display(Name = "订单标识")]
        [MaxLength(100)]
        public string order_Uid { get; set; }
        [Display(Name = "充值金额")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 0 充值 1 积分
        /// </summary>
        [Display(Name = "充值类型")]
        public MemberActivityFlag RechargeType { get; set; }
        [Display(Name = "充值规则")]
        public int RuleTypeId { get; set; }

        [Display(Name = "兑换后的金额（积分）")]
        public decimal TureAmount { get; set; }
        /// <summary>
        /// 支付状态
        /// </summary>
        public int pay_status { get; set; }
        /// <summary>
        /// 支付方式 1 微信 2 支付宝
        /// </summary>
        public int Pay_Type { get; set; }
        /// <summary>
        /// 支付订单id
        /// </summary>
        public string Prepay_Id { get; set; }
        [ForeignKey("OperatorId")]
        public Administrator Operator { get; set; }
    }
}
