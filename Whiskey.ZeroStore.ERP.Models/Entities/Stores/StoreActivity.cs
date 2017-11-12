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
    /// <summary>
    /// 实体
    /// </summary>
    [Serializable]
    public class StoreActivity : EntityBase<int>
    {
        public StoreActivity()
        {
            ActivityName = string.Empty;
            Notes = string.Empty;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            MinConsume = 0;
            StoreIds = string.Empty;
            MemberTypes = string.Empty;
            OnlyOncePerMember = false;
        }

        [Display(Name = "活动名称")]
        [Required(ErrorMessage = "不能为空！!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string ActivityName { get; set; }


        [Display(Name = "最低消费金额")]
        public virtual decimal MinConsume { get; set; }


        [Display(Name="备注")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string Notes { get; set; }

        [Display(Name="开始时间")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndDate { get; set; }


        [Display(Name = "折扣金额")]
        public virtual decimal DiscountMoney { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 活动店铺
        /// </summary>
        public virtual string StoreIds { get; set; }


        /// <summary>
        /// 目标用户
        /// </summary>
        public virtual string MemberTypes { get; set; }


        /// <summary>
        /// 如果是面向会员的店铺活动,用于限制是否每个会员只能参与1次
        /// </summary>
        [Display(Name = "每位会员只能参与1次")]
        public bool? OnlyOncePerMember { get; set; }
    }
}
