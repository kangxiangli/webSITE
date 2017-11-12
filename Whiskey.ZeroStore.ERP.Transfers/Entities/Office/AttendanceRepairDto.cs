using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AttendanceRepairDto : IAddDto, IEditDto<int>
    {

        [DisplayName("考勤")]
        public virtual int? AttendanceId { get; set; }

        [DisplayName("补卡员工")]
        [Description("申请补卡的人")]
        public virtual int? AdminId { get; set; }

        [DisplayName("扣除积分")]
        [Description("员工可以通过扣除积分来补卡")]
        public virtual double PaidScore { get; set; }

        //[DisplayName("罚款金额")]
        //public virtual double PaidMoney { get; set; }

        [DisplayName("是否支付")]
        public virtual bool IsPaid { get; set; }

        [DisplayName("补卡标识")]//参见Whiskey.ZeroStore.ERP.Transfers.Enum.Office.ApiAttenFlag
        public int ApiAttenFlag { get; set; }

        [DisplayName("已补卡")]
        public bool IsPardon { get; set; }
        /// <summary>
        /// 参考VerifyFlag
        /// </summary>
        [DisplayName("审核状态")]
        public int VerifyType { get; set; }
        /// <summary>
        /// 是否需要扣除双倍积分
        /// </summary>
        [DisplayName("是否需要扣除双倍积分")]
        [DefaultValue(false)]
        public bool IsDoubleScore { get; set; }

        [DisplayName("审核人")]
        public virtual int? VerifyAdminId { get; set; }

        [DisplayName("标识")]
        public int Id { get; set; }

        [DisplayName("申请人姓名")]
        public string RealName { get; set; }

        [DisplayName("补卡原因")]
        [DefaultValue("")]
        public virtual string Reasons { get; set; }
    }
}
