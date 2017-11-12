using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities.Warehouse
{
  public  class OrderblankItemDto: IAddDto, IEditDto<int>
  {
      public int Id { get; set; }

      [Display(Name = "所属商品")]
      public virtual int ProductId { get; set; }
      [Display(Name = "所属配货单")]
      public virtual int OrderblankId { get; set; }

      [Display(Name = "所属配货单号")]
      public string OrderblankNumber { get; set; }
      [Display(Name = "配货数量")]
      public virtual int Quantity { get; set; }
      /// <summary>
      /// 多条数据之间用,分隔
      /// </summary>
      [Display(Name = "配货商品对应条码")]
      public virtual string OrderBlankBarcodes { get; set; }

      [Display(Name = "吊牌价格")]
      public virtual float TagPrice { get; set; }

      [Display(Name = "零售价格")]
      public virtual float RetailPrice { get; set; }

      [Display(Name = "批发价格")]
      public virtual float WholesalePrice { get; set; }

      [Display(Name = "采购价格")]
      public virtual float PurchasePrice { get; set; }
    }
}
