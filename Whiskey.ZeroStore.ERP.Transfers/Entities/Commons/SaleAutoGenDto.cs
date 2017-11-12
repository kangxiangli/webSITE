
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class SaleAutoGenDto : IAddDto, IEditDto<int>
    {
        public SaleAutoGenDto()
        {
            ProductIds = new List<int>();
            ReceiveStorageIds = new List<int>();
            SellerMemberIds = new List<SellerMemberId>();
        }

        public int Id { get; set; }

        [Display(Name = "款号")]
        [Required]
        public virtual List<int> ProductIds { get; set; }

        [Display(Name = "总数")]
        [Range(1, 10000)]
        public virtual int AllSaleCount { get; set; }

        [Display(Name = "折扣")]
        [Range(1, 10)]
        public virtual decimal? Discount { get; set; }

        [Display(Name = "储值系数")]
        [Range(0,1)]
        public virtual decimal? Quotiety { get; set; }

        //----以下为入库和配货用----

        [Display(Name = "入货店铺")]
        public virtual int SendStoreId { get; set; }

        [Display(Name = "入货仓库")]
        [Required]
        public virtual int SendStorageId { get; set; }

        [Display(Name = "配货仓库")]
        [Required]
        public List<int> ReceiveStorageIds { get; set; }

        [Display(Name = "开始时间")]
        [Required]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        [Required]
        public virtual DateTime EndTime { get; set; }

        [Display(Name = "零售开始时间")]
        [Required]
        public virtual DateTime RetailStartTime { get; set; }

        [Display(Name = "零售结束时间")]
        [Required]
        public virtual DateTime RetailEndTime { get; set; }

        //----以下为零售用----
        /// <summary>
        /// 销售员和会员Ids
        /// </summary>
        public List<SellerMemberId> SellerMemberIds { get; set; }
    }

    public class SellerMemberId
    {
        public SellerMemberId()
        {
            MemberIds = new List<int>();
            SellerIds = new List<int>();
        }

        [Display(Name = "店铺")]
        public virtual int StoreId { get; set; }

        [Display(Name = "销售员")]
        public virtual ICollection<int> SellerIds { get; set; }

        [Display(Name = "购买会员")]
        public virtual ICollection<int> MemberIds { get; set; }
    }
}


