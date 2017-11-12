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
    [Description("店铺在线选货购物车详情")]
    [Serializable]
    public class StoreCartItem: EntityBase<int>
    {
        public StoreCartItem()
        {
            //初始化数量，初始值为1
            Quantity = 1;
            StoreCartItems = new List<StoreCartItem>();
        }

        [DisplayName("购物车")]
        public virtual int? StoreCartId { get; set; }

        [Display(Name = "父级")]
        [Description("同款好为一个父级")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "款号")]
        [StringLength(10)]
        [Description("同款式商品下款号是相同的")]
        public virtual string BigProdNum { get; set; }
        
        [DisplayName("商品")]
        public virtual int? ProductId { get; set; }
        
        [DisplayName("数量")]
        public virtual int Quantity { get; set; }         

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("StoreCartId")]
        public virtual StoreCart StoreCart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("ParentId")]
        public virtual StoreCartItem Parent { get; set; }

        public virtual ICollection<StoreCartItem> StoreCartItems { get; set; }
    }
}
