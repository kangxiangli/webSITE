using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities
{
    public class SalesCampaignDto : EntityBase<int>
    {
        [Display(Name = "活动编号")]
        public int CampaignNumber { get; set; }
        [Display(Name = "活动所属店铺")]
        public int? CampaignStoreId { get; set; }
        //public bool IsAllStoreCampaign { get; set; }
        [Display(Name = "活动名称")]
        public string CampaignName { get; set; }
        [Display(Name = "活动描述")]
        public string Descript { get; set; }

        [Display(Name = "活动开始时间")]
        public DateTime CampaignStartTime { get; set; }
        [Display(Name = "活动结束时间")]
        public DateTime CampaignEndTime { get; set; }
        /// <summary>
        /// 最低消费
        /// </summary>
        public double MinConsume { get; set; }

        /// <summary>
        /// 活动是否过期
        /// </summary>
        public bool ISPass { get; set; }
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
        public int SalesCampaignType { get; set; }
        [Display(Name = "会员专享折扣")]
        public float MemberDiscount { get; set; }
        [Display(Name = "非会员商品折扣")]
        public float NoMmebDiscount { get; set; }

        [Display(Name = "参与该优惠活动的商品")]
        public virtual string[] BigProdNums { get; set; }
        /// <summary>
        /// 是否所有的商品都可以参加该活动
        /// </summary>
        public bool IsAllProduct { get; set; }
    }
}
