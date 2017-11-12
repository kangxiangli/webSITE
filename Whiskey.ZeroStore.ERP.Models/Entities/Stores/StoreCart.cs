using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Description("店铺在线选货购物车详情")]
    public class StoreCart : EntityBase<int>
    {

        public StoreCart()
        {
            StoreCartItems = new List<StoreCartItem>();
        }

        [DisplayName("购物车编号")]
        [StringLength(20)]
        [Description("MD5生成唯一编码")]
        public virtual string StoreCartNum { get; set; }

        [DisplayName("采购人员")]
        [Description("选购商品的人")]
        public virtual int? PurchaserId { get; set; }       

        [DisplayName("是否订购")]
        [Description("当用户订购后，不再购物车中显示；如果没有订购，可以在对当前的购车进行调整")]
        public virtual bool IsOrder { get; set; }

        [DisplayName("来源")]
        public StoreCardOriginFlag OriginFlag { get; set; }

        #region 当StoreCardOriginFlag==临时，时启用

        [DisplayName("手机号")]
        [StringLength(15)]
        public string Phone { get; set; }

        [StringLength(20)]
        [DisplayName("姓名")]
        public string Name { get; set; }

        [StringLength(200)]
        [DisplayName("地址")]
        public string Address { get; set; }

        [DisplayName("负责人")]
        public virtual int? CaptainId { get; set; }

        [ForeignKey("CaptainId")]
        public virtual Administrator Captain { get; set; }

        #endregion

        #region 当StoreCardOriginFlag==工厂，时启用

        [DisplayName("工厂")]
        public virtual int? FactoryId { get; set; }

        [ForeignKey("FactoryId")]
        public virtual Factorys Factory { get; set; }

        #endregion

        [ForeignKey("PurchaserId")]
        public virtual Administrator Purchaser { get; set; }

        public virtual ICollection<StoreCartItem> StoreCartItems { get; set; }
    }
}
