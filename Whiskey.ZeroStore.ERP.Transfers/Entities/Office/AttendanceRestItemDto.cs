using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AttendanceRestItemDto : IAddDto, IEditDto<int>
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

        [DisplayName("标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
