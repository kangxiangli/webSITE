using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员类型实体
    /// </summary>
    [Serializable]
    public class MemberType : EntityBase<int>
    {
        public MemberType()
        {
            Members = new List<Member>();
        }

        [Display(Name = "名称")]
        [StringLength(20,ErrorMessage=("名称在20个字符以内"))]
        [Required(ErrorMessage="请填写会员类型名称")]
        public virtual string MemberTypeName { get; set; }

        [Display(Name="折扣")]
        public virtual decimal MemberTypeDiscount { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(200)]
        public virtual string Notes { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Member> Members { get; set; }

        public virtual ICollection<MemberActivity> MemberActivitys { get; set; }

        public virtual ICollection<StoreValueRule> StoreValueRules { get; set; }
    }
}
