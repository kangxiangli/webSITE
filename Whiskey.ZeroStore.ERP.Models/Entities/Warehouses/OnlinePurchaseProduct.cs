using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{

    [DisplayName("在线采购商品")]
    [Description("将商品添加到在线采购商品内，让店铺在有效期内可以采购里面的商品")]
    public class OnlinePurchaseProduct : EntityBase<int>
    {
        public OnlinePurchaseProduct()
        {
            NoticeQuantity = 1;
            OnlinePurchaseProductItems = new List<OnlinePurchaseProductItem>();
        }

        [Display(Name = "推送标题")]
        [StringLength(25)]
        public virtual string NoticeTitle { get; set; }

        [Display(Name = "推送次数")]
        public virtual int NoticeQuantity { get; set; }

        [Display(Name = "编码")]
        [StringLength(20)]
        [Description("根据当前的时期201610171636+随机数4位生成唯一标识")]
        public virtual string UniqueCode { get; set; }

        [Display(Name = "开始日期")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "结束日期")]
        public virtual DateTime EndDate { get; set; }

        [Display(Name = "推送内容")]
        [StringLength(120)]
        public virtual string NoticeContent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<OnlinePurchaseProductItem> OnlinePurchaseProductItems { get; set; }
    }

    /// <summary>
    /// 款号新品状态配置对象(在线选货及商城使用)
    /// </summary>
    public class BigProdNumStateConfigEntry
    {
        /// <summary>
        /// 新品时间,超过此时间点加入到选货单的商品都算作新品
        /// </summary>
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "新品时间")]
        public DateTime NewProductTime { get; set; }

        /// <summary>
        /// 经典商品时间,在此时间点之前的选货单中的商品都算作经典款
        /// </summary>
        [Required(ErrorMessage = "{0}不可为空")]
        [Display(Name = "经典时间")]

        public DateTime ClassicProductTime { get; set; }


        public DateTime ModifyTime { get; set; }


        public int? OperatorId { get; set; }
    }


    /// <summary>
    /// 商品款式的状态， 新品,经典,普通款
    /// </summary>
    public enum BigProdNumStateEnum
    {
        普通 = 0,
        新品 = 1,
        经典 = 2
    }


}
