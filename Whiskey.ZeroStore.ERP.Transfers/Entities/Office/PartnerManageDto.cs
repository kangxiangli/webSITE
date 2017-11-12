using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PartnerManageDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "备注")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Notes { get; set; }

        /*----------以下为加盟注册需要提交的信息--------------*/

        [Display(Name = "会员昵称")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(12, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string MemberName { get; set; }

        [Display(Name = "会员密码")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string MemberPass { get; set; }

        [Display(Name = "会员性别")]
        public virtual GenderFlag_CN Gender { get; set; }

        [Display(Name = "手机号码")]
        [StringLength(15, ErrorMessage = "不能超过15个字符")]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "电子邮箱")]
        [StringLength(50, ErrorMessage = "不能超过50个字符")]
        public virtual string Email { get; set; }

        [Display(Name = "身份证正面")]
        [StringLength(200, ErrorMessage = "不能超过200个字符")]
        public virtual string IDCard_Front { get; set; }

        [Display(Name = "身份证反面")]
        [StringLength(200, ErrorMessage = "不能超过200个字符")]
        public virtual string IDCard_Reverse { get; set; }

        [Display(Name = "所在省份")]
        [StringLength(12, ErrorMessage = "12个字符以内")]
        public string Province { get; set; }

        [Display(Name = "所在城市")]
        [StringLength(12, ErrorMessage = "12个字符以内")]
        public string City { get; set; }

        [Display(Name = "店铺地址")]
        [StringLength(50, ErrorMessage = "50个字符以内")]
        public virtual string Address { get; set; }

        [Display(Name = "经营许可证")]
        [StringLength(200)]
        public virtual string LicencePhoto { get; set; }

        [Display(Name = "店铺照片")]
        [StringLength(200)]
        public virtual string StorePhoto { get; set; }

        [Display(Name = "邮政编码")]
        [StringLength(6, ErrorMessage = "6位数字")]
        public virtual string ZipCode { get; set; }

        [Display(Name = "创建时间")]
        public virtual DateTime? CreateTime { get; set; }

        [Display(Name = "提交人")]
        public virtual int? ProposerId { get; set; }

        /// <summary>
        /// 当MemberId不会为空时，来自于 会员自己申请，否则来自于 后台主动申请
        /// </summary>
        [Display(Name = "申请会员")]
        public int? MemberId { get; set; }

        [Display(Name = "审核状态")]
        public CheckStatusFlag CheckStatus { get; set; }

        [Display(Name = "审核结果")]
        public string CheckNotes { get; set; }

        [Display(Name = "审核结果用户已读")]
        public virtual bool IsRead { get; set; }
    }
}
