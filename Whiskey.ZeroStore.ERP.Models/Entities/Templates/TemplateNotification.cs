using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class TemplateNotification : EntityBase<int>
    {
        public TemplateNotification()
        {
            Templates = new List<Template>();
        }
        [Display(Name = "通知模板名称")]
        [Required, StringLength(15)]
        public virtual string Name { get; set; }

        [Display(Name = "模板标识说明")]
        //[StringLength(150)]
        public virtual string Notes { get; set; }

        [Display(Name = "通知分类")]
        [Required]
        public virtual TemplateNotificationType NotifciationType { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [Display(Name = "该通知类型所对应的模块")]
        public virtual ICollection<Template> Templates { get; set; }
    }

    [Serializable]
    public enum TemplateNotificationType
    {
        /// <summary>
        /// 配货
        /// </summary>
        Picking,
        /// <summary>
        /// 会员储值积分修改
        /// </summary>
        ChangeRecharge,
        /// <summary>
        /// 入职通知
        /// </summary>
        Entry,
        /// <summary>
        /// 入职通知-财务审核
        /// </summary>
        EntryFinanice,
        /// <summary>
        /// 入职审核未通过
        /// </summary>
        EntryRefuse,
        /// <summary>
        /// 离职通知
        /// </summary>
        Resignation,
        /// <summary>
        /// 离职通知-财务审核
        /// </summary>
        ResignationFinanice,
        /// <summary>
        /// 离职审核未通过
        /// </summary>
        ResignationRefuse,
        /// <summary>
        /// 储值积分调整审核通知
        /// </summary>
        Changecheck,
        /// <summary>
        /// 商品审核
        /// </summary>
        ProductVerify,
        /// <summary>
        /// 商品审核结果
        /// </summary>
        ProductVerifyResult,
        /// <summary>
        /// 会员预约
        /// </summary>
        MemberAppointment = 11,
        /// <summary>
        /// 会员获取到优惠券
        /// </summary>
        MemberGetCoupons,
        /// <summary>
        /// 员工生日
        /// </summary>
        AdminBirthday,
        /// <summary>
        /// 会员生日
        /// </summary>
        MemberBirthday,



        #region 短信通知用

        /// <summary>
        /// 会员注册
        /// </summary>
        MemberRegister = 1000,
        /// <summary>
        /// 忘记密码
        /// </summary>
        MemberForgotPassword,
        /// <summary>
        /// 店铺销售
        /// </summary>
        StoreRetail,
        /// <summary>
        /// 会员储值
        /// </summary>
        MemberDeposit,
        /// <summary>
        /// 零售退货
        /// </summary>
        RetailReturn,
        /// <summary>
        /// 订餐预约
        /// </summary>
        OrderFood,

        #endregion

    }
}
