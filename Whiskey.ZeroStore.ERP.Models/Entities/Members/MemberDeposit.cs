
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class MemberDeposit : EntityBase<int>
    {
        public MemberDeposit()
        {
            Price = 0;
            Cash = 0;
            Card = 0;
            Coupon = 0;
            Score = 0;
            RechargeGenerateRecords = new List<RechargeGenerateRecord>();
        }
        [Display(Name = "充值会员")]
        public virtual int MemberId { get; set; }


        [Display(Name = "充值金额")]
        public virtual decimal Price { get; set; }

        [Display(Name = "充值积分")]
        public virtual decimal Score { get; set; }

        [Display(Name = "现金消费")]
        public virtual decimal Cash { get; set; }

        [Display(Name = "刷卡消费")]
        public virtual decimal Card { get; set; }

        [Display(Name = "优惠赠送")]
        public virtual decimal Coupon { get; set; }

        [Display(Name = "充值类型")]
        public virtual MemberDepositFlag? MemberDepositType { get; set; } //0表示系统；1表示人工

        [Display(Name = "充值场景")]
        public MemberDepositContextEnum? DepositContext { get; set; }

        [Display(Name = "活动类型")]
        public virtual MemberActivityFlag MemberActivityType { get; set; }  //0表示办卡充值 1送积分

        [Display(Name = "会员活动")]
        public virtual int? MemberActivityId { get; set; }  //记录系统充值下记录 送积分或者充值赠送积分 活动ID

        //微信扫码支付充值生成订单标识
        [Display(Name = "订单标识")]
        [MaxLength(100)]
        public virtual string order_Uid { get; set; }

        [Display(Name = "充值系数")]
        public virtual decimal Quotiety { get; set; }

        [Display(Name = "充值前余额")]
        public virtual decimal BeforeBalance { get; set; }

        [Display(Name = "充值后余额")]
        public virtual decimal AfterBalance { get; set; }

        [Display(Name = "充值前积分")]
        public virtual decimal BeforeScore { get; set; }

        [Display(Name = "充值后积分")]
        public virtual decimal AfterScore { get; set; }
        /// <summary>
        /// 针对当前充值记录的可用储值，不累加之前的记录
        /// </summary>
        [Display(Name = "可用储值")]
        public virtual decimal TotalBalance { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(200)]
        public virtual string Notes { get; set; }

        /// <summary>
        /// 充值店铺
        /// </summary>
        [Display(Name = "充值店铺")]
        public int? DepositStoreId { get; set; }


        [ForeignKey("DepositStoreId")]
        public virtual Store Store { get; set; }


        /// <summary>
        /// 订单信息
        /// </summary>
        [Index]
        [StringLength(100,ErrorMessage ="{0}不能超过100个字符")]
        [Display(Name = "订单信息")]
        public virtual string RelatedOrderNumber { get; set; }


        /// <summary>
        /// 订单类型
        /// </summary>
        [Display(Name = "订单类型")]
        public virtual OrderTypeEnum? OrderType { get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("MemberActivityId")]
        public virtual MemberActivity MemberActivity { get; set; }

        public virtual ICollection<RechargeGenerateRecord> RechargeGenerateRecords { get; set; }
    }

   

   


    
}


