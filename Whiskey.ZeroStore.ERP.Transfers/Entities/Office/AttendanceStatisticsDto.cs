using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AttendanceStatisticsDto: IAddDto, IEditDto<int>
    {
        [DisplayName("用户")]
        public virtual int AdminId { get; set; }

        [DisplayName("年份")]
        public virtual int Year { get; set; }

        [DisplayName("正数分钟")]
        public virtual double PositiveMinutes { get; set; }

        [DisplayName("负数分钟")]
        public virtual double NegativeMinutes { get; set; }

        [DisplayName("休息天数")]
        public virtual double RestDays { get; set; }

        [DisplayName("缺勤天数")]
        public virtual double AbsenceDays { get; set; }

        /// <summary>
        /// 正负时间和天数是否可用
        /// </summary>
        [DisplayName("是否有效")]
        public virtual bool IsValid { get; set; }

        [DisplayName("标识Id")]
        public virtual int Id { get; set; }
    }
}
