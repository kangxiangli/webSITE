using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 商品活动
    /// </summary>
    [Serializable]
    public class SalesCampaign : EntityBase<int>
    {
        public SalesCampaign() {
            OtherCampaign = false;
            OtherCashCoupon = true;
            ProductOriginNumbers = new List<ProductOriginNumber>();
        }
        /// <summary>
        /// 活动编号[100001-999999]
        /// </summary>
        [Display(Name = "活动编号")]
        public int CampaignNumber { get; set; }


        /// <summary>
        /// 活动名称
        /// </summary>
        [Display(Name = "活动名称")]
        public string CampaignName { get; set; }

        /// <summary>
        /// 活动描述
        /// </summary>
        [Display(Name = "活动描述")]
        public string Descript { get; set; }

        /// <summary>
        /// 活动开始时间"
        /// </summary>

        [Display(Name = "活动开始时间")]
        public DateTime CampaignStartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        [Display(Name = "活动结束时间")]
        public DateTime CampaignEndTime { get; set; }




        /// <summary>
        /// 是否可以与其他活动一起使用
        /// </summary>
        public bool OtherCampaign { get; set; }


        /// <summary>
        /// 是否可以与其他代金券一起使用
        /// </summary>
        public bool OtherCashCoupon { get; set; }

        /// <summary>
        /// 0:仅非会员可以参加 1：仅会员 2：非会员和会员都可以参加
        /// </summary>
        [Display(Name = "活动参与者")]
        public SalesCampaignType SalesCampaignType { get; set; }

        /// <summary>
        /// 会员专享折扣
        /// </summary>
        [Display(Name = "会员专享折扣")]
        public float MemberDiscount { get; set; }

        /// <summary>
        /// 非会员商品折扣
        /// </summary>
        [Display(Name = "非会员折扣")]
        public float NoMmebDiscount { get; set; }

        /// <summary>
        /// 参与活动的商品款号,与ProductOriginNumber多对多关系
        /// </summary>
        [Display(Name = "参与活动的商品款号")]
        public virtual ICollection<ProductOriginNumber> ProductOriginNumbers { get; set; }

        /// <summary>
        /// 参与活动的店铺id
        /// </summary>
        public virtual string StoresIds { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Administrator { get; set; }
    }

    /// <summary>
    /// 商品活动参与用户类型(会员/非会员/所有人)
    /// </summary>
    public enum SalesCampaignType
    {
        /// <summary>
        /// 仅非会员
        /// </summary>
        NoMemberOnly = 0,

        /// <summary>
        /// 仅会员
        /// </summary>
        MemberOnly = 1,

        /// <summary>
        /// 所有
        /// </summary>
        EveryOne = 2
    }
}
