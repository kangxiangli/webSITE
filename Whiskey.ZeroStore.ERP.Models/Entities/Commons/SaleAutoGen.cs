
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
	public class SaleAutoGen : EntityBase<int>
	{
        public SaleAutoGen()
        {
            Products = new List<Product>();
            ReceiveStorages = new List<Storage>();
            SellerMembers = new List<SellerMember>();
        }

        [Display(Name = "货号")]
        public virtual ICollection<Product> Products { get; set; }

        [Display(Name = "总数")]
        public virtual int AllSaleCount { get; set; }

        [Display(Name = "折扣")]
        [Range(1, 10)]
        public virtual decimal? Discount { get; set; }

        [Display(Name = "储值系数")]
        [Range(0, 1)]
        public virtual decimal? Quotiety { get; set; }

        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndTime { get; set; }

        [Display(Name = "零售开始时间")]
        public virtual DateTime RetailStartTime { get; set; }

        [Display(Name = "零售结束时间")]
        public virtual DateTime RetailEndTime { get; set; }

        //----以下为入库和配货用----

        [Display(Name = "入货店铺")]
        public virtual int SendStoreId { get; set; }

        [ForeignKey("SendStoreId")]
        public virtual Store SendStore { get; set; }

        [Display(Name = "入货仓库")]
        public virtual int SendStorageId { get; set; }

        [ForeignKey("SendStorageId")]
        public virtual Storage SendStorage { get; set; }

        [Display(Name = "配货仓库")]
        public virtual ICollection<Storage> ReceiveStorages { get; set; }

        //----以下为零售用----
        [Display(Name = "销售员和会员")]
        public virtual ICollection<SellerMember> SellerMembers { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }

    public class SellerMember : EntityBase<int>
    {
        public SellerMember()
        {
            Members = new List<Member>();
            Sellers = new List<Administrator>();
        }

        public virtual int SaleAutoGenId { get; set; }

        [ForeignKey("SaleAutoGenId")]
        public virtual SaleAutoGen SaleAutoGen { get; set; }

        [Display(Name = "店铺")]
        public virtual int StoreId { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public virtual ICollection<Administrator> Sellers { get; set; }

        /// <summary>
        /// 购买会员
        /// </summary>
        [Display(Name = "购买会员")]
        public virtual ICollection<Member> Members { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

