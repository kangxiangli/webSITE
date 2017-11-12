using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Attribute
{
    public static class QueueAttribute
    {
        public static Queue<Dictionary<MemberDto, MemberFigure>> queueMembers = new Queue<Dictionary<MemberDto, MemberFigure>>();

        public static Dictionary<MemberDto, MemberFigure> DicMember { get; set; }


        /// <summary>
        /// 考勤统计时间
        /// </summary>
        public static DateTime StartDate
        {
            get
            {
                string strTime = ConfigurationHelper.GetAppSetting("AttenTime");
                DateTime time = DateTime.Parse(strTime);
                return time;
            }
        }

        #region 将会员数据插入队列中
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="fig"></param>

        public static void InsertQueueMember(MemberDto dto, MemberFigure fig)
        {
            Dictionary<MemberDto, MemberFigure> dic = new Dictionary<MemberDto, MemberFigure>();
            dic.Add(dto, fig);
            queueMembers.Enqueue(dic);
        }
        #endregion

        #region 统计考勤
        /// <summary>
        /// 统计考勤
        /// </summary>
        public static void Atten()
        {
            IAdministratorContract _adminContract = EntityContract._adminContract;
            IAttendanceContract _attenContract = EntityContract._attenContract;
            IAttendanceStatisticsContract _attenStatisticsContract = EntityContract._attenStatisticsContract;
            DateTime current = DateTime.Now;
            List<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            List<Attendance> listAtten = _attenContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AttendanceTime.Year == current.Year && x.AttendanceTime.Month == current.Month && x.AttendanceTime.Day == current.Day).ToList();
            List<int> listAdminId = listAdmin.Select(x => x.Id).ToList();
            List<AttendanceStatistics> listStatistics = _attenStatisticsContract.AttendanceStatisticses.Where(x => listAdminId.Contains(x.AdminId)).ToList();
            Dictionary<string, int> dic = GetHoliday();
            WorkTime workTime;
            List<Attendance> listEntity = new List<Attendance>();
            List<AttendanceStatistics> listStatisticsAdd = new List<AttendanceStatistics>();
            List<AttendanceStatistics> listStatisticsUpdate = new List<AttendanceStatistics>();

            foreach (Administrator admin in listAdmin)
            {
                workTime = GetWorkTime(admin);
                bool res = OfficeHelper.IsWorkDay(current, workTime, dic);
                if (res == true)
                {
                    Attendance atten = GetAtten(listAtten, current, admin.Id, workTime.IsPersonalTime);
                    if (atten.Id == 0)
                    {
                        listEntity.Add(atten);
                    }
                    AttendanceStatistics statistics = GetAttenStatistics(listStatistics, workTime, atten, admin.Id);
                    if (statistics.Id == 0)
                    {
                        listStatisticsAdd.Add(statistics);
                    }
                    else
                    {
                        listStatisticsUpdate.Add(statistics);
                    }
                }
                else
                {
                    string strDate = current.ToString("yyyy/MM/dd 00:00:00");
                    DateTime date = DateTime.Parse(strDate);
                    IOvertimeContract _overTimeContract = DependencyResolver.Current.GetService<IOvertimeContract>();
                    int count = _overTimeContract.Overtimes.Where(x => x.AdminId == admin.Id && x.VerifyType == (int)VerifyFlag.Pass && date.CompareTo(x.StartTime) >= 0 && date.CompareTo(x.EndTime) <= 0).Count();
                    if (count > 0)
                    {
                        Attendance atten = GetAtten(listAtten, current, admin.Id, workTime.IsPersonalTime);
                        if (atten != null)
                        {
                            listEntity.Add(atten);
                        }
                    }
                }
            }

            _attenContract.Add(listEntity, listStatisticsAdd, listStatisticsUpdate);
        }


        #endregion

        #region 获取假期

        private static Dictionary<string, int> GetHoliday()
        {

            IHolidayContract _holidayContract = EntityContract._holidayContract;
            Dictionary<string, int> dic = _holidayContract.GetHoliday();
            return dic;
        }

        #endregion

        #region 获取工作时间

        private static WorkTime GetWorkTime(Administrator admin)
        {
            WorkTime workTime = new WorkTime();
            if (admin.IsPersonalTime)
            {
                workTime = admin.WorkTime;
                workTime.IsPersonalTime = admin.IsPersonalTime;
            }
            else
            {
                workTime = admin.JobPosition.WorkTime;
            }
            return workTime;
        }

        #endregion

        #region 获取考勤数据

        private static Attendance GetAtten(List<Attendance> listAtten, DateTime current, int adminId, bool IsPersonalTime)
        {
            Attendance atten = listAtten.FirstOrDefault(x => x.AdminId == adminId);
            if (atten == null)
            {
                atten = new Attendance()
                {
                    AdminId = adminId,
                    AttendanceTime = current,
                    IsAbsence = -1,
                    AbsenceType = (int)AttendanceFlag.DayOfAbsence
                };
            }
            return atten;
        }

        #endregion

        #region 获取统计考勤数据
        private static AttendanceStatistics GetAttenStatistics(List<AttendanceStatistics> listStatistics, WorkTime workTime, Attendance atten, int adminId)
        {
            AttendanceStatistics statistics = listStatistics.FirstOrDefault(x => x.AdminId == adminId && x.CreatedTime.Year == atten.CreatedTime.Year);
            int hour = workTime.WorkHour;
            int negativeMinutes = 0;
            int positiveMinutes = 0;
            if (workTime.IsPersonalTime)
            {
                var deatile = EntityContract._workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTime.Id && x.WorkDay == DateTime.Now.Day)
                      .FirstOrDefault();
                if (deatile != null)
                {
                    hour = deatile.WorkHour;
                }
            }
            if (statistics == null)
            {
                statistics = new AttendanceStatistics()
                {
                    AdminId = adminId,
                };
            }
            else
            {
                //negativeMinutes = statistics.NegativeMinutes;
                //positiveMinutes = statistics.PositiveMinutes;
            }
            negativeMinutes = (int)(atten.LateMinutes + negativeMinutes + atten.LeaveEarlyMinutes);
            positiveMinutes = (int)(atten.ArrivalEarlyMinutes + positiveMinutes + atten.LeaveLateMinutes);
            if (atten.IsAbsence == -1)
            {
                if (atten.AbsenceType == (int)AttendanceFlag.AmAbsence || atten.AbsenceType == (int)AttendanceFlag.PmAbsence)
                {
                    int mimutes = 0;
                    if (workTime.IsFlexibleWork == true)
                    {
                        mimutes = 4 * 60;
                    }
                    else
                    {
                        mimutes = hour / 2 * 60;
                    }
                    negativeMinutes = negativeMinutes + mimutes;
                }
                else
                {
                    statistics.AbsenceDays = statistics.AbsenceDays + 1;
                }
            }
            statistics.NegativeMinutes = negativeMinutes + statistics.NegativeMinutes;
            statistics.PositiveMinutes = positiveMinutes + statistics.PositiveMinutes;
            if (workTime.IsFlexibleWork == false && statistics.NegativeMinutes > 60 * hour)
            {
                int workMin = 60 * hour;
                int neDays = statistics.NegativeMinutes / workMin;
                int neMins = statistics.NegativeMinutes % workMin;
                statistics.AbsenceDays = statistics.AbsenceDays + neDays;
                statistics.NegativeMinutes = neMins;
            }
            if (workTime.IsFlexibleWork == false && statistics.PositiveMinutes > 60 * hour)
            {
                int workMin = 60 * hour;
                int poDays = statistics.PositiveMinutes / workMin;
                int poMins = statistics.PositiveMinutes % workMin;
                statistics.RestDays = statistics.RestDays + poDays;
                statistics.PositiveMinutes = poMins;
            }
            return statistics;
        }
        #endregion

        #region 发送订餐预约信息

        public static bool SendOrderFoodSms(List<string> listphones)
        {
            if (listphones.Count > 0)
            {
                var nowDate = DateTime.Now.Date;
                var isWorkDay = new int[] { 1, 2, 3, 4, 5 }.Contains((int)nowDate.DayOfWeek);
                if (isWorkDay)
                {
                    var modOF = EntityContract._OrderFoodContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.CreatedTime >= nowDate).OrderByDescending(o => o.Id).FirstOrDefault();
                    if (modOF.IsNotNull())
                    {
                        if (modOF.smsIsSend)
                        {
                            return true;
                        }
                        var list = modOF.Admins.Where(w => w.IsEnabled && !w.IsDeleted).GroupBy(g => g.Department.DepartmentName).Select(s => new
                        {
                            DepName = s.Key,
                            Count = s.Count()
                        }).Where(w => w.Count > 0).ToList();
                        if (list.Count > 0)
                        {
                            modOF.smsIsSend = true;
                            var res = EntityContract._OrderFoodContract.Update(modOF);
                            if (res.ResultType == OperationResultType.Success)
                            {
                                var strinfo = string.Join(",", list.Select(s => $"{s.DepName}:{s.Count}"));
                                var dic = new Dictionary<string, object>(){
                                        { "DepNameAndCount",strinfo },
                                        { "AllCount",list.Sum(s=>s.Count) },
                                    };
                                foreach (var phone in listphones)
                                {
                                    EntityContract._SmsContract.SendSms(phone, TemplateNotificationType.OrderFood, dic);
                                }
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}