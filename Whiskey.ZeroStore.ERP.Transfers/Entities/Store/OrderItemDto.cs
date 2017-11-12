﻿
//   This file was generated by T4 code generator Dto_Creater.tt.



using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OrderItemDto : IAddDto, IEditDto<int>
    {
		[Display(Name = "所属店铺")]
		public Int32 StoreId { get; set; }

		[Display(Name = "所属订单")]
		public Int32 OrderId { get; set; }

		[Display(Name = "所属商品")]
		public Int32 ProductId { get; set; }

		[Display(Name = "吊牌价格")]
		public Decimal TagPrice { get; set; }

		[Display(Name = "零售价格")]
		public Decimal RetailPrice { get; set; }

		[Display(Name = "商品数量")]
		public Int32 Quantity { get; set; }

		[Display(Name = "获得积分")]
		public Int32 ObtainScore { get; set; }

		[Display(Name = "实体标识")]
		public Int32 Id { get; set; }

		[Display(Name = "排序序号")]
		public Int32 Sequence { get; set; }

    }
}