using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class UpdateStoreActivityDto
    {
        public UpdateStoreActivityDto()
        {
            ActivityName = string.Empty;
            Notes = string.Empty;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            MinConsume = int.MaxValue;
            StoreIds = new int[] { };
            MemberTypes = new int[] { };

        }

        public int Id { get; set; }

        [Display(Name = "活动名称")]
        [Required(ErrorMessage = "不能为空！!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string ActivityName { get; set; }


        [Display(Name = "最低消费金额")]
        public virtual decimal MinConsume { get; set; }


        [Display(Name = "备注")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string Notes { get; set; }

        [Display(Name = "开始时间")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndDate { get; set; }


        [Display(Name = "折扣金额")]
        public virtual decimal DiscountMoney { get; set; }



        /// <summary>
        /// 活动店铺
        /// </summary>
        public virtual int[] StoreIds { get; set; }


        /// <summary>
        /// 目标用户
        /// </summary>
        public virtual int[] MemberTypes { get; set; }
    }
}