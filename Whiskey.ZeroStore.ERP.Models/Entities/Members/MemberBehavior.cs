using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MemberBehavior : EntityBase<int>
    {
        public MemberBehavior()
        {
            PageViews = 1;
        }

        [Display(Name = "会员")]
        [Index]
        public virtual int MemberId { get; set; }

        [Display(Name = "商品")]
        [Index]
        public virtual int ProductOriginNumberId { get; set; }

        [Display(Name = "品类")]
        [Index]
        public virtual int CategoryId { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        [Display(Name = "浏览次数")]
        public virtual int PageViews { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ProductOriginNumberId")]
        public virtual ProductOriginNumber ProductOriginNumber { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

    }
}
