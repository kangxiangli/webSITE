
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
    public class Express : EntityBase<int>
    {
        public Express() {

        }

        [Display(Name = "快递公司")]
        public virtual string CompanyName { get; set; }

        [Display(Name = "快递代号")]
        public virtual string ExpressCode { get; set; }

        [Display(Name = "快递运费")]
        public virtual decimal ExpressFee { get; set; }//会员额外支付费用

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


