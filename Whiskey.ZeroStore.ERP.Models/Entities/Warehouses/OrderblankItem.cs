using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{ //yxk 2015-10-9
    /// <summary>
    /// 配货明细
    /// </summary>
    public class OrderblankItem : EntityBase<int>
    {


        [Display(Name = "所属商品")]
        public virtual int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }


        [Display(Name = "所属配货单")]
        public virtual int? OrderblankId { get; set; }
        [ForeignKey("OrderblankId")]
        public virtual Orderblank Orderblank { get; set; }


        [Display(Name = "所属配货单号")]
        public string OrderblankNumber { get; set; }


        [Display(Name = "配货数量")]
        public virtual int Quantity { get; set; }


        /// <summary>
        /// 多条数据之间用,分隔
        /// </summary>
        [Display(Name = "配货商品对应条码")]
        public virtual string OrderBlankBarcodes { get; set; }




    }
}
