
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class MemberConsume : EntityBase<int>
    {
        public MemberConsume()
        {

        }

        /// <summary>
        /// 消费类型,用于区分是储值消费还是积分消费
        /// </summary>
        [Display(Name = "消费类型")]
        public MemberActivityFlag ConsumeType { get; set; }

        /// <summary>
        /// 消费会员
        /// </summary>
        [Display(Name = "消费会员")]
        public virtual int MemberId { get; set; }


        /// <summary>
        /// 消费店铺
        /// </summary>
        [Display(Name = "消费店铺")]
        public virtual int StoreId { get; set; }



        /// <summary>
        /// 消费场景
        /// </summary>
        [Display(Name = "消费场景")]
        [Required(ErrorMessage = "{0}不能为空")]
        public MemberConsumeContextEnum? ConsumeContext { get; set; }




        /// <summary>
        /// 消费储值
        /// </summary>
        [Display(Name = "消费储值")]
        public virtual decimal BalanceConsume { get; set; }



        /// <summary>
        /// 消费积分
        /// </summary>
        [Display(Name = "消费积分")]
        public virtual decimal ScoreConsume { get; set; }


        /// <summary>
        /// 订单信息
        /// </summary>
        [Index]
        [Display(Name = "订单信息")]
        [StringLength(100, ErrorMessage = "{0}不能超过100个字符")]
        public virtual string RelatedOrderNumber { get; set; }


        /// <summary>
        /// 订单类型
        /// </summary>
        [Display(Name = "订单类型")]
        public virtual OrderTypeEnum? OrderType{ get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }

    
}


