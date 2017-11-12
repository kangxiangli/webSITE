using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Models
{
    public class SignRuleInfo 
    {
        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 父级Id
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string SignRuleName {get;set;}

        /// <summary>
        /// 星期
        /// </summary>
        public int  Week {get;set;}

        /// <summary>
        /// 奖品类型
        /// </summary>
        public int PrizeType {get;set;}
        /// <summary>
        /// 积分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 优惠券
        /// </summary>
        public string CouponName{get;set;}
        /// <summary>
        /// 奖品
        /// </summary>
        public string PrizeName {get;set;}

        public bool IsDeleted { get; set; }

        public bool IsEnabled { get; set; }
    }
}