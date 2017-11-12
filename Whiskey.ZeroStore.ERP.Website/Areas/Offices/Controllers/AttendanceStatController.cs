using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    /// <summary>
    /// 考勤统计
    /// </summary>
    public class AttendanceStatController : Controller
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly ILeaveInfoContract _leaveInfoContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IOvertimeContract _overtimeContract;

        protected readonly IFieldContract _fieldContract;

        protected readonly IAttendanceRestItemContract _attRestItemContract;

        protected readonly IAttendanceStatisticsContract _attStatisticsContract;

        protected readonly IHolidayContract _holidayContract;
        public AttendanceStatController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            ILeaveInfoContract leaveInfoContract,
            IWorkTimeContract workTimeContract,
            IOvertimeContract overtimeContract,
            IFieldContract fieldContract,
            IAttendanceRestItemContract attRestItemContract,
            IAttendanceStatisticsContract attStatisticsContract,
            IHolidayContract holidayContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _leaveInfoContract = leaveInfoContract;
            _workTimeContract = workTimeContract;
            _overtimeContract = overtimeContract;
            _fieldContract = fieldContract;
            _attRestItemContract = attRestItemContract;
            _attStatisticsContract = attStatisticsContract;
            _holidayContract = holidayContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化数据界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            #region 是否显示部门考勤

            bool isPower = false;
            int count = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.SubordinateId == adminId).Count();
            int index = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.JobPosition.IsLeader == true).Count();
            if (count > 0 || index > 0)
            {
                isPower = true;
            }
            #endregion
            Rest rest = _restContract.Rests.FirstOrDefault(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true);
            AttendanceStatistics attStat = _attStatisticsContract.AttendanceStatisticses.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId);
            double annualLeaveDays = 0;
            double paidLeaveDays = 0;
            double absenceDays = 0;
            double restDays = 0;
            double _leaveDays = 0;
            var leave_list = _leaveInfoContract.LeaveInfos.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1);
            if (leave_list.Count() > 0)
            {
                _leaveDays = leave_list.Sum(x => x.LeaveDays);
            }
            double _overtimeDays = 0;
            var overtime_list = _overtimeContract.Overtimes.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1);
            if (overtime_list.Count() > 0)
            {
                _overtimeDays = overtime_list.Sum(x => x.OvertimeDays);
            }
            double _fieldDays = 0;
            var field_list = _fieldContract.Fields.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1);
            if (field_list.Count() > 0)
            {
                _fieldDays = field_list.Sum(x => x.FieldDays);
            }
            if (attStat != null)
            {
                restDays = attStat.RestDays;
                absenceDays = attStat.AbsenceDays;
            }
            if (rest != null)
            {
                annualLeaveDays = rest.AnnualLeaveDays;
                paidLeaveDays = rest.PaidLeaveDays;
            }
            ViewBag.Power = isPower;
            ViewBag.AnnualLeaveDays = annualLeaveDays + "天";
            ViewBag.PaidLeaveDays = paidLeaveDays + "天";
            ViewBag.LeaveDays = _leaveDays + "小时";
            ViewBag.OvertimeDays = _overtimeDays + "小时";
            ViewBag.FieldDays = _fieldDays + "小时";
            return View();
        }
        #endregion

        #region 获取数据
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    int adminId = AuthorityHelper.OperatorId ?? 0;        
        //    var data = await Task.Run(() =>
        //    {
        //        var count = 0;                             
        //        IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true);
        //        //取出考勤年份
        //        List<int> listYear = listAttendance.Select(x => x.AttendanceTime.Year).Distinct().OrderByDescending(x=>x).ToList();
        //        List<AttendanceInfo> listAttendanceInfo = new List<AttendanceInfo>();
        //        Administrator admin = _administratorContract.View(adminId);
        //        int workHour = admin.JobPosition.WorkTime.WorkHour;
        //        workHour = workHour == 0 ? 1 : workHour;
        //        string[] weeks = admin.JobPosition.WorkTime.WorkWeek.Split(',');
        //        int id=1;
        //        foreach (int year in listYear)
        //        {
        //            //循环1-12月
        //            for (int i = 1; i <= 12; i++)
        //            {
        //                List<Attendance> listAtten = listAttendance.Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == i).ToList();                        
        //                if (listAtten.Count>0)
        //                {
        //                    int currentDays = DateTime.DaysInMonth(year, i);
        //                    int absenceDays = 0;                                
        //                    for (int j = 1; j <= currentDays; j++)
        //                    {
        //                        DateTime currentDate = DateTime.Parse(year.ToString() + '/' + i.ToString() + '/' + j.ToString());
        //                        count = listAtten.Where(x => x.AttendanceTime.Day == j).Count();
        //                        if (count == 0)
        //                        {
        //                            absenceDays += 1;
        //                        }
        //                    }
        //                    double lateMinutes = listAtten.Select(x => x.LateMinutes).Sum();
        //                    double leaveEarlyMinutes = listAtten.Select(x => x.LeaveEarlyMinutes).Sum();
        //                    double arrivalEarlyMinutes = listAtten.Select(x => x.ArrivalEarlyMinutes).Sum();
        //                    double LeaveLateMinutes = listAtten.Select(x => x.LeaveLateMinutes).Sum();
        //                    double minute = arrivalEarlyMinutes + LeaveLateMinutes - lateMinutes - leaveEarlyMinutes;
        //                    double days = Math.Round((minute *1.0 / 60 / workHour), 1);
        //                    int leaveAllDays = listAtten.Where(x => x.LeaveInfoId != null && x.LeaveInfoType == (int)AttendanceFlag.DayOfLeave).Count();
        //                    double leaveHarfDays = listAtten.Where(x => x.LeaveInfoId != null && (x.LeaveInfoType == (int)AttendanceFlag.AmLeave || x.LeaveInfoType == (int)AttendanceFlag.PmLeave)).Count() / (double)2;
        //                    int absenceAllDays = listAtten.Where(x => x.IsAbsence == true && x.AbsenceType == (int)AttendanceFlag.DayOfAbsence).Count();
        //                    double absenceHarfDays = (listAtten.Where(x => x.IsAbsence == true && (x.AbsenceType == (int)AttendanceFlag.AmAbsence || x.AbsenceType == (int)AttendanceFlag.PmAbsence)).Count()) / (double)2;
        //                    absenceHarfDays += listAtten.Where(x => x.AmStartTime==null).Count() / (double)2;
        //                    int overAllDays = listAtten.Where(x => x.OvertimeId != null && x.OvertimeType == (int)AttendanceFlag.DayOfOvertime).Count();
        //                    double overHarfDays = listAtten.Where(x => x.OvertimeId != null && (x.OvertimeType == (int)AttendanceFlag.AmOvertime || x.OvertimeType == (int)AttendanceFlag.PmOvertime)).Count() / (double)2;
        //                    int fieldAllDays = listAtten.Where(x => x.FieldId != null && x.FieldType == (int)AttendanceFlag.DayOfField).Count();
        //                    double fieldHarfDays = listAtten.Where(x => x.FieldId != null && (x.FieldType == (int)AttendanceFlag.AmField || x.FieldType == (int)AttendanceFlag.PmField)).Count() / (double)2;
        //                    double normalDays = listAttendance.Where(x => x.LeaveInfoId == null && x.IsAbsence == false && x.OvertimeId==null).Count();                            
        //                    AttendanceInfo attenInfo = new AttendanceInfo()
        //                    {
        //                        Id=id,
        //                        Date = year.ToString() + "年" + i.ToString() + "月",
        //                        LateCount = listAtten.Where(x => x.IsLate == true).Count(),
        //                        LeaveEarlyCount = listAtten.Where(x => x.IsLeaveEarly == true).Count(),
        //                        LeaveDays = leaveAllDays + leaveHarfDays,
        //                        AbsenceDays = absenceDays + absenceAllDays,
        //                        OvertimeDays=overAllDays+overHarfDays,
        //                        FieldDays=fieldAllDays+fieldHarfDays,
        //                        NormalDays =0, 
        //                        Minutes=minute,
        //                        Days = days
        //                    };
        //                    listAttendanceInfo.Add(attenInfo);
        //                    id++;
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }    
        //        }
        //        int pageIndex = request.PageCondition.PageIndex;
        //        int pageSize = request.PageCondition.PageSize;
        //        var list = listAttendanceInfo.Skip(pageIndex*pageSize).Take(pageSize).Select(x => new
        //        {
        //            x.Id,
        //            x.Date,
        //            x.LateCount,
        //            x.LeaveEarlyCount,
        //            x.LeaveDays,
        //            x.AbsenceDays,
        //            x.NormalDays,
        //            x.Minutes,
        //            x.Days,
        //            x.FieldDays,
        //            x.OvertimeDays
        //        });
        //        count = list.Count();
        //        return new GridData<object>(list, count, request.RequestInfo);
        //    });
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string start_Str = Request["satrtTime"];
            string end_Str = Request["endTime"];
            DateTime startTime = string.IsNullOrEmpty(start_Str) ? DateTime.Now : DateTime.Parse(start_Str);
            DateTime endTime = string.IsNullOrEmpty(end_Str) ? DateTime.Now : DateTime.Parse(end_Str);
            string type_str = Request["type"];
            if (type_str == "1")
            {
                startTime = DateTime.Parse(DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-01 0:00");
                endTime = DateTime.Parse(DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + " 23:59");
            }
            else if (type_str == "2")
            {
                int c_year = DateTime.Now.Year;
                int c_month = DateTime.Now.Month;
                if (c_month == 1)
                {
                    c_year = c_year - 1;
                    c_month = 12;
                }
                else
                {
                    c_month = c_month - 1;
                }
                startTime = DateTime.Parse(c_year + "-" + c_month.ToString().PadLeft(2, '0') + "-01 0:00");
                endTime = DateTime.Parse(c_year + "-" + c_month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(c_year, c_month) + " 23:59");
            }
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var data = await Task.Run(() =>
            {
                int count = 0;
                List<Attendance> listAtten = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId).ToList();
                Administrator admin = _administratorContract.View(adminId);
                int startYear = startTime.Year;
                int startMonth = startTime.Month;
                int EndYear = endTime.Year;
                int endMonth = endTime.Month;
                int month = 12;
                List<AttendanceInfo> listAttInfo = new List<AttendanceInfo>();
                while (true)
                {
                    if (startYear > EndYear)
                    {
                        break;
                    }
                    else
                    {
                        if (startYear == EndYear)
                        {
                            month = endMonth;
                        }
                        int _count = 0;
                        int _currentM = startMonth;
                        for (int j = startYear; startYear <= EndYear; j++)
                        {
                            if (j == EndYear)
                            {
                                if (_count > 0 && startYear == EndYear)
                                {
                                    _currentM = 1;
                                }
                                else
                                {
                                    _currentM = startMonth;
                                }
                                for (int i = _currentM; i <= endMonth; i++)
                                {
                                    List<Attendance> list = listAtten.Where(x => x.AttendanceTime.Year == startYear && x.AttendanceTime.Month == i).ToList();
                                    AttendanceInfo attenInfo = CalcDays(list);
                                    attenInfo.Date = startYear.ToString() + "年" + i.ToString() + "月";
                                    attenInfo.NormalDays = CalNormalDays(j, i, adminId);
                                    attenInfo.OvertimeDays = GetOvertimeDaysCount(j, i, adminId);
                                    attenInfo.FieldDays = GetFiledDaysCount(j, i, adminId);
                                    attenInfo.LeaveDays = GetLeaveDaysCount(j, i, adminId);
                                    attenInfo.LateCount = GetLateCount(j, i, adminId);
                                    attenInfo.LeaveEarlyCount = GetLeaveEarlyCount(j, i, adminId);
                                    attenInfo.NoSignOutCount = GetNoSignOutCount(j, i, adminId);
                                    listAttInfo.Add(attenInfo);
                                }
                            }
                            else
                            {
                                if (_count > 0)
                                {
                                    _currentM = 1;
                                }
                                for (int i = _currentM; i <= 12; i++)
                                {
                                    List<Attendance> list = listAtten.Where(x => x.AttendanceTime.Year == startYear && x.AttendanceTime.Month == i).ToList();
                                    AttendanceInfo attenInfo = CalcDays(list);
                                    attenInfo.Date = startYear.ToString() + "年" + i.ToString() + "月";
                                    attenInfo.NormalDays = CalNormalDays(j, i, adminId);
                                    attenInfo.OvertimeDays = GetOvertimeDaysCount(j, i, adminId);
                                    attenInfo.FieldDays = GetFiledDaysCount(j, i, adminId);
                                    attenInfo.LateCount = GetLateCount(j, i, adminId);
                                    attenInfo.LeaveEarlyCount = GetLeaveEarlyCount(j, i, adminId);
                                    attenInfo.LeaveDays = GetLeaveDaysCount(j, i, adminId);
                                    attenInfo.NoSignOutCount = GetNoSignOutCount(j, i, adminId);
                                    listAttInfo.Add(attenInfo);
                                }
                                _count = _count + 1;
                            }
                            startYear++;
                        }

                    }
                }
                return new GridData<object>(listAttInfo, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region  注释--算出本月缺勤
        //private double GetAbsenceDays(int adminId,int year,int month)
        //{
        //    Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
        //    if (admin.JobPosition!=null)
        //    {
        //        string[] weeks = admin.JobPosition.WorkTime.WorkWeek.Split(',');
        //        int currentMonths = DateTime.DaysInMonth(year, month);
        //        DateTime startDate = DateTime.Parse(year.ToString() + "/" + month.ToString() + "/" + "1");
        //        DateTime endDate = DateTime.Parse(year.ToString() + "/" + month.ToString() + "/" + currentMonths.ToString());
        //        int restDays=GetRestDay(weeks,startDate,endDate);
        //        double workDays = currentMonths - restDays;
        //        IQueryable<Attendance> listAtt =  _attendanceContract.Attendances.Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == month && x.AdminId==adminId && x.IsDeleted==false &&x.IsEnabled==true);
        //        double allDays = listAtt.Where(x => x.OvertimeId == null && x.LeaveInfoId == null && x.IsAbsence == false).Count();
        //        double harlfDays = listAtt.Where(x => x.OvertimeId == null && x.LeaveInfoId == null && x.IsAbsence == false).Count();
        //    }
        //    return 0; 
        //}
        #endregion

        #region 注释-获取休假时间
        /// <summary>
        /// 获取休假时间
        /// </summary>
        /// <param name="weeks"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        //private int GetRestDay(string[] weeks, DateTime startDate, DateTime endDate)
        //{
        //    int restDay = 0;
        //    int index = 0;
        //    while (true)
        //    {
        //        DateTime tempDate = startDate.AddDays(index);
        //        if (tempDate.Year == endDate.Year && tempDate.Month == endDate.Month && tempDate.Day == endDate.Day)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            string strCurrentWeek = tempDate.DayOfWeek.ToString("d");
        //            string strCurrentDate = tempDate.ToString("yyyyMMdd");
        //            if (weeks.Contains(strCurrentWeek))
        //            {
        //                Dictionary<string, string> dic = VacationsHelper.GetVacations();
        //                if (dic.ContainsKey(strCurrentDate))
        //                {
        //                    if (dic[strCurrentDate] == ((int)WorkDateFlag.Holidays).ToString())
        //                    {
        //                        restDay += 1;
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Dictionary<string, string> dic = VacationsHelper.GetVacations();
        //                if (dic.ContainsKey(strCurrentDate))
        //                {
        //                    if (dic[strCurrentDate] == ((int)WorkDateFlag.WorkDay).ToString())
        //                    {
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        restDay += 1;
        //                    }
        //                }
        //            }
        //        }
        //        index += 1;
        //    }
        //    return restDay;
        //}
        #endregion

        #region 获取正常上班数据

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Normal()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString();
            ViewBag.EndTime = endTime.ToString();
            ViewBag.AdminId = adminId;
            return PartialView();
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NormalList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled);
                var listEntity = listAttendance.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Member.RealName,
                    x.AmStartTime,
                    x.PmEndTime,
                    x.CreatedTime,
                    x.AttendanceTime,
                    x.ArrivalEarlyMinutes,
                    x.LeaveLateMinutes,
                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #endregion

        #region 获取上班迟到数据

        #region 初始化迟到界面
        /// <summary>
        /// 初始化迟到界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Late()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            ViewBag.AdminId = adminId;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.EndTime = endTime.ToString("yyyy-MM-dd hh:mm:ss");
            return PartialView();
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LateList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.IsLate == -1);
                var listEntity = listAttendance.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Member.RealName,
                    x.AmStartTime,
                    x.PmEndTime,
                    x.IsLate,
                    x.IsLeaveEarly,
                    x.IsAbsence,
                    x.IsDeleted,
                    x.LeaveInfoId,
                    x.AttendanceTime,
                    x.LateMinutes,
                    x.LeaveEarlyMinutes,
                    x.IsNoSignOut
                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 获取上班早退数据

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaveEarly()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.EndTime = endTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.AdminId = adminId;
            return PartialView();
        }

        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LeaveEarlyList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.IsLeaveEarly == -1);
                var listEntity = listAttendance.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Member.RealName,
                    x.AmStartTime,
                    x.PmEndTime,
                    x.IsLate,
                    x.IsLeaveEarly,
                    x.IsAbsence,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.CreatedTime,
                    x.LeaveInfoId,
                    x.AttendanceTime,
                    x.LateMinutes,
                    x.LeaveEarlyMinutes,

                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 获取上班分钟数
        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Minutes()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            IQueryable<Attendance> listAtt = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId && x.AttendanceTime.Year == date.Year && x.AttendanceTime.Month == date.Month);
            Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            WorkTime workTime = admin != null && admin.JobPosition != null ? admin.JobPosition.WorkTime : null;
            double arrivalEarlyMinutes = listAtt.Select(x => x.ArrivalEarlyMinutes).Sum();
            double lateMinutes = listAtt.Select(x => x.LateMinutes).Sum();
            double leaveEarlyMinutes = listAtt.Select(x => x.LeaveEarlyMinutes).Sum();
            double leaveLateMinutes = listAtt.Select(x => x.LeaveLateMinutes).Sum();
            double totalMinutes = arrivalEarlyMinutes + leaveLateMinutes - lateMinutes - leaveEarlyMinutes;
            AttendanceRestItem attRest = _attRestItemContract.AttendanceRestItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId && x.ExchangeDate.Year == date.Year && x.ExchangeDate.Month == date.Month).FirstOrDefault();
            bool IsExchange = true;
            if (attRest == null)
            {
                IsExchange = false;
            }
            int hour = workTime != null && !workTime.IsFlexibleWork ? workTime.WorkHour : 1;
            //if ( workTime.IsFlexibleWork == true)
            //{
            //    hour = 1;
            //}
            int days = (int)totalMinutes / (hour * 60);
            ViewBag.TotalMinutes = totalMinutes;
            ViewBag.Days = days;
            ViewBag.StartTime = startTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.EndTime = endTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.AdminId = adminId;
            ViewBag.IsExchange = IsExchange;
            return PartialView();
        }

        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> MinutesList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.ArrivalEarlyMinutes != 0 || x.LateMinutes != 0 || x.LeaveEarlyMinutes != 0 && x.LeaveLateMinutes != 0);
                var listEntity = listAttendance.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Member.RealName,
                    x.AmStartTime,
                    x.PmEndTime,
                    x.IsLate,
                    x.IsLeaveEarly,
                    x.IsAbsence,
                    x.CreatedTime,
                    x.LeaveInfoId,
                    x.OvertimeId,
                    x.AttendanceTime,
                    x.LateMinutes,
                    x.LeaveEarlyMinutes,
                    x.ArrivalEarlyMinutes,
                    x.LeaveLateMinutes,
                    TotalMinutes = (x.ArrivalEarlyMinutes + x.LeaveLateMinutes - x.LateMinutes - x.LeaveEarlyMinutes),
                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #endregion

        #region 获取缺勤数据

        #region 初始化界面

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Absence()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            ViewBag.AdminId = adminId;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            int pardonCount = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId && x.AttendanceTime.Year == date.Year && x.AttendanceTime.Month == date.Month && x.IsPardon == true).Count();
            pardonCount = 3 - pardonCount;
            ViewBag.PardonCount = pardonCount;
            ViewBag.StartTime = startTime.ToString("yyyy-MM-dd hh:mm:ss");
            ViewBag.EndTime = endTime.ToString("yyyy-MM-dd hh:mm:ss");
            return PartialView();
        }
        #endregion

        #region 获取数据
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AbsenceList()
        {
            GridRequest request = new GridRequest(Request);
            string strAdminId = request.FilterGroup.Rules.Where(x => x.Field == "AdminId").FirstOrDefault().Value.ToString();
            string strAttendanceTime = request.FilterGroup.Rules.Where(x => x.Field == "AttendanceTime").FirstOrDefault().Value.ToString();
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                int adminId = int.Parse(strAdminId);
                DateTime attendanceTime = DateTime.Parse(strAttendanceTime);
                var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
                WorkTime workTime = admin != null && admin.JobPosition != null ? admin.JobPosition.WorkTime : null;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true);
                listAttendance = listAttendance.Where(x => x.AttendanceTime.Year == attendanceTime.Year && x.AttendanceTime.Month == attendanceTime.Month);
                string[] weeks = workTime != null && workTime.WorkWeek != null ? workTime.WorkWeek.Split(',') : null;
                int currentDays = DateTime.DaysInMonth(attendanceTime.Year, attendanceTime.Month);
                List<Attendance> listAtt = new List<Attendance>();
                for (int j = 1; j <= currentDays; j++)
                {
                    DateTime currentDate = DateTime.Parse(attendanceTime.Year.ToString() + "/" + attendanceTime.Month.ToString() + "/" + j.ToString());
                    if (workTime.IsVacations == true)
                    {
                        Dictionary<string, int> dic = _holidayContract.GetHoliday();
                        bool isWorkDay = OfficeHelper.IsWorkDay(currentDate, workTime, dic);
                        if (isWorkDay == false)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        string strWeek = currentDate.DayOfWeek.ToString("d");
                        if (weeks == null || weeks.Where(x => x == strWeek).Count() == 0)
                        {
                            continue;
                        }
                    }
                    Attendance atten = listAttendance.Where(x => x.AttendanceTime.Day == j).FirstOrDefault();
                    if (atten == null)
                    {
                        atten = new Attendance()
                        {
                            AdminId = adminId,
                            AttendanceTime = DateTime.Parse(attendanceTime.Year.ToString() + "/" + attendanceTime.Month.ToString() + "/" + j.ToString()),
                            IsAbsence = -1,
                            AbsenceType = (int)AttendanceFlag.DayOfAbsence,
                            IsDeleted = false,
                            IsEnabled = true,
                            IsPardon = false,
                        };
                        listAtt.Add(atten);
                        continue;
                    }

                    if ((atten.IsAbsence == -1 || string.IsNullOrEmpty(atten.AmStartTime)) || atten.IsNoSignOut == -1)
                    {
                        listAtt.Add(atten);
                    }
                }
                var listEntity = listAtt.AsQueryable().Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    //x.Administrator.RealName,
                    //x.AmStartTime,
                    //x.PmEndTime,                    
                    x.IsAbsence,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.AttendanceTime,
                    x.AbsenceType,
                    x.IsPardon,
                    PardonCount = listAtt.Where(k => k.IsPardon == true).Count(),
                    x.IsNoSignOut
                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 获取请假数据

        #region 初始化数据界面
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Leave()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            strDate = string.IsNullOrEmpty(strDate) ? DateTime.Now.ToString() : strDate;
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString();
            ViewBag.EndTime = endTime.ToString();
            ViewBag.AdminId = adminId;
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> LeaveList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<LeaveInfo, bool>> predicate = FilterHelper.GetExpression<LeaveInfo>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _leaveInfoContract.LeaveInfos.Where<LeaveInfo, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.VacationType,
                    x.VerifyType,
                    x.LeaveDays,
                    x.AnnualLeaveDays,
                    x.ChangeRestDays,
                    x.PaidLeaveDays,
                    x.LeaveMethod,
                    x.UseAnnualLeaveDay
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #endregion

        #region 获取加班数据

        #region 初始化数据界面
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Overtime()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString();
            ViewBag.EndTime = endTime.ToString();
            ViewBag.AdminId = adminId;
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> OvertimeList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Overtime, bool>> predicate = FilterHelper.GetExpression<Overtime>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<OvertimeRestItem> listOvertimeRestItem = _overtimeContract.OvertimeRestItems;
                var list = _overtimeContract.Overtimes.Where<Overtime, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.OvertimeDays,
                    x.VerifyType,
                    Count = listOvertimeRestItem.Where(y => y.OvertimeId == x.Id).Count() > 0
                });

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #endregion

        #region 获取外勤数据

        #region 初始化数据界面
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Field()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            strDate = string.IsNullOrEmpty(strDate) ? DateTime.Now.ToString() : strDate;
            DateTime date = DateTime.Parse(strDate);
            DateTime startTime = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
            int maxMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime endTime = new DateTime(date.Year, date.Month, maxMonth, 23, 59, 59);
            ViewBag.StartTime = startTime.ToString();
            ViewBag.EndTime = endTime.ToString();
            ViewBag.AdminId = adminId;
            return PartialView();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> FieldList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Field, bool>> predicate = FilterHelper.GetExpression<Field>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _fieldContract.Fields.Where<Field, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.FieldDays,
                    x.FieldWorkDays,
                    x.VerifyType,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #endregion

        #region 兑换天数
        public JsonResult Exchange()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strDate = Request["Date"];
            DateTime date = DateTime.Parse(strDate);
            var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            WorkTime workTime = admin != null ? admin.JobPosition.WorkTime : null;
            DateTime nowDate = DateTime.Now;
            if (nowDate.Year <= date.Year && nowDate.Month <= date.Month)
            {
                return Json(new OperationResult(OperationResultType.Error, "当前月份不能进行兑换"));
            }
            if (workTime.IsFlexibleWork == true)
            {
                return Json(new OperationResult(OperationResultType.Error, "弹性工作时间无法兑换"));
            }
            else
            {
                AttendanceRestItem attRest = _attRestItemContract.AttendanceRestItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId && x.ExchangeDate.Year == date.Year && x.ExchangeDate.Month == date.Month).FirstOrDefault();
                if (attRest == null)
                {
                    IQueryable<Attendance> listAtt = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId && x.AttendanceTime.Year == date.Year && x.AttendanceTime.Month == date.Month);
                    double arrivalEarlyMinutes = listAtt.Select(x => x.ArrivalEarlyMinutes).Sum();
                    double lateMinutes = listAtt.Select(x => x.LateMinutes).Sum();
                    double leaveEarlyMinutes = listAtt.Select(x => x.LeaveEarlyMinutes).Sum();
                    double leaveLateMinutes = listAtt.Select(x => x.LeaveLateMinutes).Sum();
                    int hour = workTime.WorkHour;
                    double totalMinutes = arrivalEarlyMinutes + leaveLateMinutes - lateMinutes - leaveEarlyMinutes;
                    int days = (int)totalMinutes / (hour * 60);
                    if (days <= 0)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "没有达到兑换的标准"));
                    }
                    else
                    {
                        double surplusMinutes = totalMinutes - days * 8 * 60;
                        AttendanceRestItemDto attRestDto = new AttendanceRestItemDto()
                        {
                            ExchangeDate = date,
                            AdminId = adminId,
                            Days = days,
                            Minutes = totalMinutes,
                            SurplusMinutes = surplusMinutes
                        };
                        var res = _attRestItemContract.Insert(attRestDto);
                        return Json(res);
                    }
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "本月已经兑换过了"));
                }
            }

        }
        #endregion

        #region 兑换加班
        public JsonResult ExchangeOvertime(int Id)
        {
            OperationResult oper = _overtimeContract.ExchangeOvertime(Id);
            return Json(oper);
        }
        #endregion

        #region 忘记打卡
        public JsonResult Pardon()
        {
            try
            {

                string strDate = Request["Date"];
                DateTime currentDate = DateTime.Parse(strDate);
                var res = _attendanceContract.Pardon(currentDate);
                return Json(res);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后"));
            }

        }
        #endregion

        #region 计算每月考勤数据
        private AttendanceInfo CalcDays(List<Attendance> list)
        {
            double absenceDays = 0;
            double leaveDays = 0;
            double overTimeDays = 0;
            double fieldDays = 0;
            absenceDays = list.Where(x => x.AbsenceType == (int)AttendanceFlag.DayOfAbsence).Count();
            absenceDays = absenceDays + list.Where(x => x.AbsenceType == (int)AttendanceFlag.AmAbsence || x.AbsenceType == (int)AttendanceFlag.PmAbsence).Count() / 2;
            leaveDays = list.Where(x => x.LeaveInfoId != null && x.LeaveInfoType == (int)AttendanceFlag.DayOfLeave).Count();
            leaveDays = leaveDays + list.Where(x => x.LeaveInfoId != null && x.LeaveInfoType == (int)AttendanceFlag.AmLeave || x.LeaveInfoType == (int)AttendanceFlag.PmLeave).Count() / 2;
            overTimeDays = list.Where(x => x.OvertimeId != null && x.OvertimeType == (int)AttendanceFlag.DayOfOvertime).Count();
            overTimeDays = overTimeDays + list.Where(x => x.OvertimeId != null && x.OvertimeType == (int)AttendanceFlag.AmOvertime || x.OvertimeType == (int)AttendanceFlag.PmOvertime).Count() / 2;
            fieldDays = list.Where(x => x.FieldId != null && x.FieldType == (int)AttendanceFlag.DayOfField).Count();
            fieldDays = fieldDays + list.Where(x => x.FieldId != null && x.FieldType == (int)AttendanceFlag.AmField || x.FieldType == (int)AttendanceFlag.PmField).Count() / 2;
            AttendanceInfo attenInfo = new AttendanceInfo()
            {
                AbsenceDays = absenceDays,
                FieldDays = fieldDays,
                OvertimeDays = overTimeDays,
                LeaveDays = leaveDays,
            };
            return attenInfo;
        }
        #endregion

        #region 计算每月正常上班时间
        private int CalNormalDays(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _attendanceContract.Attendances.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.AttendanceTime >= st && x.AttendanceTime <= end);
            return _attence_list.Count();
        }
        #endregion

        private double GetOvertimeDaysCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _overtimeContract.Overtimes.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.StartTime >= st && x.EndTime <= end && x.VerifyType == 1);
            double count = 0;
            if (_attence_list.Count() > 0)
            {
                count = _attence_list.Sum(x => x.OvertimeDays);
            }
            return count;
        }

        private double GetFiledDaysCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _fieldContract.Fields.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.StartTime >= st && x.EndTime <= end && x.VerifyType == 1);
            double count = 0;
            if (_attence_list.Count() > 0)
            {
                count = _attence_list.Sum(x => x.FieldDays);
            }
            return count;
        }

        /// <summary>
        /// 获取迟到次数
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="startMonth"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private int GetLateCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _attendanceContract.Attendances.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.AttendanceTime >= st && x.AttendanceTime <= end && x.IsLate == -1);
            return _attence_list.Count();
        }
        /// <summary>
        /// 获取早退次数
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="startMonth"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private int GetLeaveEarlyCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _attendanceContract.Attendances.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.AttendanceTime >= st && x.AttendanceTime <= end && x.IsLeaveEarly == -1);
            return _attence_list.Count();
        }
        /// <summary>
        /// 获取未签退次数
        /// </summary>
        /// <param name="startYear"></param>
        /// <param name="startMonth"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private int GetNoSignOutCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            //var _attence_list = _attendanceContract.Attendances.Where(x => x.AdminId == adminId&& !x.IsDeleted && x.IsEnabled && x.AttendanceTime >= st && x.AttendanceTime <= end && x.GetIsNoSignOut == -1);
            var count = _attendanceContract.Attendances.Count(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.AttendanceTime >= st && x.AttendanceTime <= end && x.IsNoSignOut == -1);
            return count;
        }
        private double GetLeaveDaysCount(int startYear, int startMonth, int adminId)
        {
            DateTime st = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-01 0:00");
            int current_day = DateTime.DaysInMonth(startYear, startMonth);
            DateTime end = DateTime.Parse(startYear.ToString() + "-" + startMonth.ToString().PadLeft(2, '0') + "-" + current_day.ToString().PadLeft(2, '0') + " 23:59");
            var _attence_list = _leaveInfoContract.LeaveInfos.Where(x => x.AdminId == adminId
            && !x.IsDeleted && x.IsEnabled && x.StartTime >= st && x.EndTime <= end && x.VerifyType == 1);
            double count = 0;
            if (_attence_list.Count() > 0)
            {
                count = _attence_list.Sum(x => x.LeaveDays);
            }
            return count;
        }

        #region 获取工作时间
        private WorkTime GetWorkTime(Administrator admin)
        {
            if (admin == null)
            {
                return null;
            }
            WorkTime workTime = new WorkTime();
            if (admin.WorkTimeId != null && workTime.IsEnabled == true)
            {
                workTime = admin.WorkTime;
            }
            else if (admin.JobPosition != null)
            {
                workTime = admin.JobPosition.WorkTime;
            }
            return workTime;
        }
        #endregion
    }
}