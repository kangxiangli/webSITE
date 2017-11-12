using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 分钟转换成调休记录
    /// </summary>
    public class AttendanceRestItem : EntityBase<int>
    {
        [DisplayName("兑换日期")]
        public virtual DateTime ExchangeDate { get; set; }

        [DisplayName("用户")]
        public virtual int AdminId { get; set; }

        [DisplayName("兑换分钟数")]
        public virtual double Minutes { get; set; }

        [DisplayName("剩余分钟数")]
        public virtual double SurplusMinutes { get; set; }

        [DisplayName("兑换的天数")]
        public virtual int Days { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
