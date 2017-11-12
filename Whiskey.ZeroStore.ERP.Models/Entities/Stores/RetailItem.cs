using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    /// <summary>
    /// 零售明细
    /// </summary>
    public class RetailItem : EntityBase<int>
    {
        public RetailItem()
        {
            RetailInventorys = new List<RetailInventory>();
        }
        /// <summary>
        /// 所属零售单
        /// </summary>
        public virtual int RetailId { get; set; }
        /// <summary>
        /// 商品
        /// </summary>
        public virtual int ProductId { get; set; }
      
        /// <summary>
        /// 出货仓库，多个用逗号分隔开
        /// </summary>
        [StringLength(100)]
        public virtual string OutStorageIds { get; set; }
        /// <summary>
        /// 商品活动id
        /// </summary>
        public virtual int? SalesCampaignId { get; set; }
        /// <summary>
        /// 享有的活动折扣
        /// </summary>
        public virtual decimal SalesCampaignDiscount { get; set; }

        /// <summary>
        /// 享有的品牌折扣
        /// </summary>
        public virtual decimal BrandDiscount { get; set; }

        /// <summary>
        /// 商品吊牌价
        /// </summary>
        public virtual decimal ProductTagPrice { get; set; }
        /// <summary>
        /// 单件商品获得积分
        /// </summary>
        [Display(Name = "单件商品获得积分")]
        [Obsolete("已弃用",true)]
        public virtual decimal GetScore { get; set; }
        /// <summary>
        /// 商品零售价
        /// </summary>
        public virtual decimal ProductRetailPrice { get; set; }
        /// <summary>
        /// 总消费金额
        /// </summary>
        public virtual decimal ProductRetailItemMoney { get; set; }
        /// <summary>
        /// 商品数量
        /// </summary>
        public virtual int RetailCount { get; set; }
        /// <summary>
        /// 状态 0:正常 1：整单退货 2：部分退货 
        /// </summary>
        public RetailStatus RetailItemState { get; set; }

        [Obsolete("已废弃",true)]
        /// <summary>
        /// 退货数量
        /// </summary>
        public virtual int ReturnedCount { get; set; }

        [ForeignKey("SalesCampaignId")]
        public virtual SalesCampaign SalesCampaign { get; set; }

        [ForeignKey("RetailId")]
        public virtual Retail Retail { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        

        /// <summary>
        /// 是否已反馈
        /// </summary>
        public bool HasFeedback { get; set; }


        /// <summary>
        /// 关联的商品库存信息
        /// </summary>
        public virtual ICollection<RetailInventory> RetailInventorys { get; set; }



   
    }
}
