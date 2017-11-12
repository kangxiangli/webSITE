



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
    public class Payment : EntityBase<int>
    {
        public Payment() {

        }

        [Display(Name = "支付名称")]
        public virtual string PaymentName { get; set; }

        [Display(Name = "支付商家ID")]
        public virtual string PaymentPartner { get; set; }

        [Display(Name = "支付密钥")]
        public virtual string PaymentKey { get; set; }

        [Display(Name = "手续费用")]
        public virtual decimal PaymentFee { get; set; }//会员额外支付费用

        [Display(Name = "联系电话")]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "公司网站")]
        public virtual string Website { get; set; }

        [Display(Name = "备注信息")]
        public virtual string Notes { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


