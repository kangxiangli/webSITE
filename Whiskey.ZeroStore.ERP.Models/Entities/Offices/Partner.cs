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
    /// 合作商
    /// </summary>
    public class Partner : EntityBase<int>
    {
        public Partner()
        {
            LoginTime = DateTime.Now;
            IsCooperation = true;
            Coupons = new List<Coupon>();
            PartnerExperiences = new List<PartnerExperience>();
        }

        [Display(Name = "合作商等级")]
        public virtual int? PartnerLevelId { get; set; } 

        [Display(Name = "合作商名称")]
        [StringLength(20)]
        public virtual string PartnerName { get; set; }

        [Display(Name = "合作商密码")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string PartnerPass { get; set; }

        [Display(Name = "合作商地址")]
        [StringLength(100)]
        public virtual string PartnerAddress { get; set; }

        [Display(Name = "合作商编码")]
        [StringLength(6)]
        public virtual string PartnerNum { get; set; }

        [Display(Name = "合作商照片")]
        [StringLength(100)]
        public virtual string PartnerPhoto { get; set; }

        [Display(Name = "电子邮箱")]
        [StringLength(50, ErrorMessage = "不能超过{1}个字符")]
        public virtual string Email { get; set; }

        [Display(Name = "联系人")]
        [StringLength(10, ErrorMessage = "不能超过{1}个字符")]
        public virtual string Contacts { get; set; }

        [Display(Name = "手机号码")]
        [StringLength(15, ErrorMessage = "不能超过{1}个字符")]
        public virtual string PhoneNum { get; set; }

        [Display(Name = "联系电话")]
        [StringLength(15, ErrorMessage = "不能超过{1}个字符")]
        public virtual string TelPhone { get; set; }

        //[Display(Name = "审核状态")]
        //public virtual int VerifyType { get; set; }

        [Display(Name = "合作状态")]
        public virtual bool IsCooperation { get; set; }

        [Display(Name = "登录次数")]
        public virtual long LoginCount { get; set; }

        [Display(Name = "登录时间")]
        public virtual DateTime LoginTime { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "图标")]
        [StringLength(100,ErrorMessage=("最大成都不能超过{1}"))]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("PartnerLevelId")]
        public virtual PartnerLevel PartnerLevel { get; set; }

        public virtual ICollection<Coupon> Coupons { get; set; }

        public virtual ICollection<PartnerExperience> PartnerExperiences { get; set; }
        
    }
}
