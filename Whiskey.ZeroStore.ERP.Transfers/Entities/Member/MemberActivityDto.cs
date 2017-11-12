using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberActivityDto : IAddDto, IEditDto<int>
    {
        public MemberActivityDto()
        {
            MemberTypes = new List<MemberType>();
        }

        [Display(Name = "充值活动名称")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public  string ActivityName { get; set; }

        [Display(Name = "充值金额")]
        public  decimal Price { get; set; }

        [Display(Name = "积分")]
        [Required(ErrorMessage = "不能为空！")]
        public  decimal Score { get; set; }       

        [Display(Name = "开始时间")]
        public  DateTime StartDate { get; set; }

        [Display(Name = "结束时间")]
        public  DateTime EndDate { get; set; }

        [Display(Name = "是否永久有效")]
        public  bool IsForever { get; set; } //false表示否，true表示是

        [Display(Name="备注")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public  string Notes { get; set; }

        [Display(Name = "活动类型")]
        public virtual int ActivityType { get; set; } //0表示充值，1表示送积分

        [Display(Name = "赠送储值")]
        public virtual decimal RewardMoney { get; set; }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        public bool IsAllStore { get; set; }



        [Display(Name = "是否所有店铺")]

        public string StoreIds { get; set; }

        public  ICollection<MemberType> MemberTypes { get; set; }
    }
}
