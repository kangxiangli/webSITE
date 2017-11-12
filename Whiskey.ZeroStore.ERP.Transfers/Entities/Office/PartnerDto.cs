using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    /// <summary>
    /// 合作商
    /// </summary>
    public class PartnerDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "合作商名称")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "至少{2}～{1}个字符")]
        [Required(ErrorMessage = "合作商名称不能为空！")]
        public virtual string PartnerName { get; set; }

        [Display(Name = "合作商密码")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "至少{2}～{1}个字符")]
        [Compare("SecondPass", ErrorMessage = "输入两次密码不一致")]
        public virtual String PartnerPass { get; set; }

        [Display(Name = "确认密码")]
        [Required(ErrorMessage = "不能为空！")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "至少{2}～{1}个字符")]
        [Compare("PartnerPass", ErrorMessage = "输入两次密码不一致")]
        public virtual string SecondPass { get; set; }        

        [Display(Name = "合作商地址")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "至少{2}～{1}个字符")]
        [Required(ErrorMessage = "合作商地址不能为空！")]        
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
        [StringLength(10, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        [Required(ErrorMessage = "联系人名称不能为空！")]   
        public virtual string Contacts { get; set; }

        [Display(Name = "手机号码")]        
        [StringLength(15, ErrorMessage = "至少{1}个字符")]
        [Required(ErrorMessage = "手机号码不能为空！")] 
        public virtual string PhoneNum { get; set; }

        [Display(Name = "联系电话")]
        [StringLength(15, ErrorMessage = "不能超过{1}个字符")]
        public virtual string TelPhone { get; set; }

        //[Display(Name = "审核状态")]
        //public virtual int VerifyType { get; set; }

        [Display(Name = "是否合作")]
        public virtual bool IsCooperation { get; set; }        

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "图标")]
        [StringLength(100, ErrorMessage = ("最大成都不能超过{1}"))]
        public virtual string IconPath { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
