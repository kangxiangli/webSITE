using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体
    /// </summary>
    [Serializable]
    public class MemberActivity : EntityBase<int>
    {
        public MemberActivity()
        {
            ActivityName = string.Empty;
            Score = 0;
            //MemberTypeId = 0;
            Notes = string.Empty;
            MemberDeposits = new List<MemberDeposit>();
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            Price = 0;
        }

        [Display(Name = "充值活动名称")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string ActivityName { get; set; }

        [Display(Name = "积分")]
        [Required(ErrorMessage = "不能为空！")]
        public virtual decimal Score { get; set; }

        [Display(Name = "充值金额")]
        public virtual decimal Price { get; set; }

        //[Display(Name = "会员类型")]
        //[Required(ErrorMessage = "不能为空！")]       
        //public virtual int MemberTypeId { get; set; }

        [Display(Name="备注")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string Notes { get; set; }

        [Display(Name="开始时间")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndDate { get; set; }
        
        [Display(Name = "是否永久有效")]
        public virtual bool IsForever { get; set; } //false表示否，true表示是

        [Display(Name = "活动类型")]
        public virtual MemberActivityFlag ActivityType { get; set; } //0表示充值，1表示送积分

        [Display(Name = "赠送储值")]
        public virtual decimal RewardMoney { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<MemberDeposit> MemberDeposits { get; set; }

        public virtual ICollection<MemberType> MemberTypes { get; set; }


        /// <summary>
        /// 是否所有店铺 
        /// </summary>
        [Display(Name = "是否所有店铺")]
        public bool IsAllStore { get; set; }


        /// <summary>
        /// 参与店铺 
        /// </summary>
        [Display(Name = "参与店铺")]
        public string StoreIds { get; set; }
    }
}
