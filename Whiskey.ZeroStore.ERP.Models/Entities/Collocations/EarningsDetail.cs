using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Collocations
{
    public class EarningsDetail:EntityBase<int>
    {
        
        [Display(Name = "消费单号")]
        public int ConsumeOrderId { get; set; }
       
        [Display(Name="消费总额")]
        public decimal Totalexpendamount { get; set; }
        /// <summary>
        /// 1：推荐会员 2：为会员推荐搭配 3：被会员预约
        /// </summary>
        [Display(Name="收益类型")]
        public int EarningsType { get; set; }
        /// <summary>
        /// 收益占消费总额的百分比
        /// </summary>
        [Display(Name="收益提成")]
        public decimal EarningsPercent { get; set; }
        /// <summary>
        /// 收益备注
        /// </summary>
        public string EarningsNotes { get; set; }
        /// <summary>
        /// 是否已结算
        /// </summary>
        public bool IsCloseAnAccount { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public DateTime CloseAnAccountTime { get; set; }
        /// <summary>
        /// 结算操作人
        /// </summary>
        public int CloseAnAccountAdmin { get; set; }
        [ForeignKey("ConsumeOrderId")]
        public Order ConsumeOrder { get; set; }
    }
}
