using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class AttendanceDto : IAddDto, IEditDto<int>, ICloneable
    {
        [DisplayName("员工Id")]
        public virtual int AdminId { get; set; }

        [DisplayName("请假信息")]
        public virtual int? LeaveInfoId { get; set; }

        [DisplayName("请假类型")]
        public virtual int LeaveInfoType { get; set; }

        [DisplayName("休假类型")]
        public virtual int VacationType { get; set; }

        [DisplayName("外勤信息")]
        public virtual int? FieldId { get; set; }

        [DisplayName("外勤类型")]
        public virtual int FieldType { get; set; }
        [DisplayName("加班信息")]
        public virtual int? OvertimeId { get; set; }

        [DisplayName("加班类型")]
        public virtual int OvertimeType { get; set; }

        [DisplayName("是否迟到")]
        [Description("-1:迟到；0：正常；1：补卡审核成功")]
        [DefaultValue(0)]
        public virtual int IsLate { get; set; }

        [DisplayName("是否早退")]
        [Description("-1:早退；0：正常；1：补卡审核成功")]
        [DefaultValue(0)]
        public virtual int IsLeaveEarly { get; set; }

        [DisplayName("是否旷工")]
        [Description("-1:旷工；0：正常；1：补卡审核成功")]
        [DefaultValue(-1)]
        public virtual int IsAbsence { get; set; }

        [DisplayName("是否未签退")]
        [Description("-1:未签退；0：正常；1：补卡审核成功")]
        [DefaultValue(-1)]
        public virtual int IsNoSignOut { get; set; }

        [DisplayName("旷工类型")]
        public virtual int AbsenceType { get; set; }

        [DisplayName("上午签到时间")]
        [StringLength(10)]
        public virtual string AmStartTime { get; set; }

        [DisplayName("下午签退时间")]
        [StringLength(10)]
        public virtual string PmEndTime { get; set; }

        [DisplayName("考勤时间")]
        public virtual DateTime AttendanceTime { get; set; }

        [DisplayName("上班时间")]
        public virtual string NormalAMStartTime { get; set; }

        [DisplayName("下班时间")]
        public virtual string NormalPMEndTime { get; set; }

        [DisplayName("早到分钟数")]
        public virtual double ArrivalEarlyMinutes { get; set; }

        [DisplayName("晚退分钟数")]
        public virtual double LeaveLateMinutes { get; set; }

        [DisplayName("迟到分钟数")]
        public virtual double LateMinutes { get; set; }

        [DisplayName("早退分钟数")]
        public virtual double LeaveEarlyMinutes { get; set; }        

        [DisplayName("赦免")] //当用户忘记打卡时导致矿工，可以有3次机会改变为正常上班
        public virtual bool IsPardon { get; set; }

        [DisplayName("标识Id")]
        public Int32 Id { get; set; }

        /// <summary>
        /// 获取用户是否未签退（因存在当天还没下班的情况存在，所以判断是否未签退用该方法）
        /// </summary>
        [NotMapped]
        public virtual int GetIsNoSignOut
        {
            get
            {
                if (AttendanceTime.Date < DateTime.Now.Date)
                {
                    return IsNoSignOut;
                }
                decimal normalPMtime = Convert.ToDateTime(AttendanceTime.ToShortDateString() + " " + NormalPMEndTime).Ticks;
                decimal nowTime = DateTime.Now.Ticks;

                return normalPMtime <= nowTime ? IsNoSignOut : 0;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <returns></returns>
        public AttendanceDto DeepClone()
        {

            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, this);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as AttendanceDto;
            }
        }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public AttendanceDto ShallowClone()
        {
            return Clone() as AttendanceDto;
        }
    }
}
