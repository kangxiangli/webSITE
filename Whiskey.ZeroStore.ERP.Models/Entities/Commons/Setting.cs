



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Setting : EntityBase<int>
    {
        public Setting() {
			
        }

        [Display(Name = "消费多少金额获得1积分")]
        public virtual int SystemMemberConsumeGetScore { get; set; }

        [Display(Name = "推荐一个会员获得多少积分")]
        public virtual int SystemRecommandUserGetScore { get; set; }

        [Display(Name = "是否用余额消费可获得积分")]
        public virtual bool SystemIsBalanceConsumeGetScore { get; set; }

        [Display(Name = "是否销售商品后自动打印")]
        public virtual bool SystemIsPrintWhenSaleProduct { get; set; }

        [Display(Name = "是否会员充值后自动打印")]
        public virtual bool SystemIsPrintWhenMemberDeposit { get; set; }

        [Display(Name = "是否开启库存预警")]
        public virtual bool SystemIsInventoryWarning { get; set; }

        [Display(Name = "到达库存预警数量")]
        public virtual bool SystemInventoryWarningNumber { get; set; }


        //========================快递设置============================

        [Display(Name = "快递合作商名称")]
        public virtual string DeliveryPartner { get; set; }

        [Display(Name = "快递合作商密钥")]
        public virtual string DeliveryKey { get; set; }

        [Display(Name = "快递API请求路径")]
        public virtual string DeliveryAPIUrl { get; set; }

        [Display(Name = "快递API公司参数名")]
        public virtual string DeliveryAPICompany { get; set; }

        [Display(Name = "快递API订单号参数名")]
        public virtual string DeliveryAPIOrderNumber { get; set; }

        [Display(Name = "快递API其他附加参数名")]
        public virtual string DeliveryAPIOtherParams { get; set; }



        //========================短信设置============================

        [Display(Name = "短信合作商名称")]
        public virtual string SMSPartner { get; set; }

        [Display(Name = "短信合作商帐号")]
        public virtual string SMSPartnerAccount { get; set; }

        [Display(Name = "短信合作商密钥")]
        public virtual string SMSKey { get; set; }

        [Display(Name = "短信API请求路径")]
        public virtual string SMSAPIUrl { get; set; }

        [Display(Name = "短信API手机号名参数名")]
        public virtual string SMSAPIMobilePhone { get; set; }

        [Display(Name = "短信API短信内容参数名")]
        public virtual string SMSAPIMessageContent { get; set; }

        [Display(Name = "短信内容字数限制")]
        public virtual int SMSNumberOfWordLimit { get; set; }



        //========================会员设置=============================

        [Display(Name = "是否开启会员生日提醒")]
        public virtual bool MemberIsBirthdayWarning { get; set; }

        [Display(Name = "会员生日提醒提前天数")]
        public virtual int MemberBirthdayWarningAdvancedDays { get; set; }

        [Display(Name = "是否开启会员生日自动短信祝贺")]
        public virtual bool MemberIsSendSMSOnBirthday { get; set; }

        [Display(Name = "会员生日自动发送祝贺短信内容")]
        public virtual string MemberSendSMSContent { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


