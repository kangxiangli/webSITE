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
    [DisplayName("在线采购商品详情")]
    [Description("可采购商品详情")]
    [Serializable]
    public class OnlinePurchaseProductItem:EntityBase<int>
    {
        [Display(Name = "在线采购商品")]
        public virtual int? OnlinePurchaseProductId { get; set; }

        [Display(Name="商品款号")]
        public virtual string BigProdNum { get; set; }

        [ForeignKey("OnlinePurchaseProductId")]
        public virtual OnlinePurchaseProduct OnlinePurchaseProduct { get; set; }
    }
}
