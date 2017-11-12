
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
    public class MemberGift : EntityBase<int>
    {
        public MemberGift()
        {

        }

        [Display(Name = "兑换会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "兑换店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "消耗积分")]
        public virtual int Score { get; set; }

        [Display(Name = "备注信息")]
        public virtual string Notes { get; set; }


        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


    }
}


