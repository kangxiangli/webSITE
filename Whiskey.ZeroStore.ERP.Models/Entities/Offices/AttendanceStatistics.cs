using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class AttendanceStatistics : EntityBase<int>
    {
        [DisplayName("用户")]
        public virtual int AdminId { get; set; }

        //[DisplayName("年份")]
        //public virtual int Year { get; set; }

        /// <summary>
        /// 正数分钟
        /// </summary>
        [DisplayName("正数分钟")]
        public virtual int PositiveMinutes { get; set; }

        /// <summary>
        /// 负数分钟
        /// </summary>
        [DisplayName("负数分钟")]
        public virtual int NegativeMinutes { get; set; }

        [DisplayName("休息天数")]
        public virtual double RestDays { get; set; }

        [DisplayName("缺勤天数")]
        public virtual double AbsenceDays { get; set; }

        /// <summary>
        /// 正负时间和天数是否可用
        /// </summary>
        [DisplayName("是否有效")] 
        public virtual bool IsValid { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
