using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AdjustDepositDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "调整原因")]
        [StringLength(20, ErrorMessage = "字符串长度不能超过{1}")]
        [Required(ErrorMessage = "请填写原因")]
        public virtual string Reason { get; set; }

        [Display(Name = "会员调整")]
        [Required(ErrorMessage = "请选择会员")]
        public virtual int? MemberId { get; set; }

        [Display(Name = "会员")]
        [Required(ErrorMessage = "请选择会员")]
        public virtual string MemberName { get; set; }

        [Display(Name = "申请人")]
        public virtual int? ApplicantId { get; set; }

        [Display(Name = "审核人")]
        public virtual int? ReviewersId { get; set; }

        [Display(Name = "减少储值")]
        [Required(ErrorMessage = "请选择填写储值金额")]
        public virtual decimal Balance { get; set; }

        [Display(Name = "减少积分")]
        [Required(ErrorMessage = "请选择填写积分")]
        public virtual decimal Score { get; set; }

        [Display(Name = "审核状态")]
        public virtual int VerifyType { get; set; }

        [Display(Name = "备注")]
        [StringLength(200, ErrorMessage = "字符串长度不能超过{1}")]
        public virtual string Notes { get; set; }

        [Display(Name = "标识Id")]
        public virtual int Id { get; set; }
    }
}
