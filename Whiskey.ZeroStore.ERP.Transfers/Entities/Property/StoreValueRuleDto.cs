using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreValueRuleDto : EntityBase<int>, IAddDto,IEditDto<int>
    {
        public StoreValueRuleDto()
        {
            MemberTypes = new List<MemberType>();
        }

        /// <summary>
        /// 100001-999999 六位数编号
        /// </summary>
        [Display(Name = "储值规则编号")]
        public int StoreValueNumber { get; set; }
        //储值规则
        /// <summary>
        /// 储值类型： 0 储值 1 积分
        /// </summary>
        [Display(Name = "充值规则类型")]
        public int RuleType { get; set; }

        [Display(Name = "充值规则名称")]
        [MaxLength(20)]
        public string StoreValueName { get; set; }
        [Display(Name = "备注")]
        [MaxLength(200)]
        public string Descrip { get; set; }

        [Display(Name = "充值金额")]
        public decimal Price { get; set; }
        [Display(Name = "赠送储值")]
        public decimal RewardMoney { get; set; }
        [Display(Name = "积分")]
        [Required(ErrorMessage = "不能为空！")]
        public virtual decimal Score { get; set; }
        [Display(Name = "是否永久有效")]
        public bool IsForever { get; set; } //false表示否，true表示是
        [Display(Name = "展示图url")]
        [DefaultValue("")]
        public string ImageUrl { get; set; }
        [Display(Name = "开始时间")]
        public DateTime StartDate { get; set; }

        [Display(Name = "结束时间")]
        public DateTime EndDate { get; set; }

        public ICollection<MemberType> MemberTypes { get; set; }

    }
}
