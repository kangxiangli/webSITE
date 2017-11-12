using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices
{
    public class WorkTimeController : Controller
    {
        protected readonly IAdministratorContract _adminContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IDepartmentContract _departContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IWorkTimeContract _workTime;
        protected readonly IHolidayContract _holidayContract;
        protected readonly ILeaveInfoContract _leaveInfoContract;
        protected readonly IOvertimeContract _overtimeContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IFieldContract _fieldContract;
        protected readonly IEntryContract _entryContract;
        protected readonly IRestContract _restContract;

        public WorkTimeController(IAdministratorContract adminContract, IStoreContract storeContract,
            IDepartmentContract departContract,
            IMemberContract memberContract,
            IAdministratorContract administratorContract,
            IWorkTimeContract workTime,
            IHolidayContract holidayContract,
            ILeaveInfoContract leaveInfoContract,
            IOvertimeContract overtimeContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IFieldContract fieldContract,
            IEntryContract entryContract,
            IRestContract restContract)
        {
            _adminContract = adminContract;
            _storeContract = storeContract;
            _departContract = departContract;
            _memberContract = memberContract;
            _workTime = workTime;
            _leaveInfoContract = leaveInfoContract;
            _overtimeContract = overtimeContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _fieldContract = fieldContract;
            _holidayContract = holidayContract;
            _entryContract = entryContract;
            _restContract = restContract;
        }

        public JsonResult GetUserSameDayWorkTimeInfo(int adminId)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
            var msg = string.Empty;
            var msgType = 0;
            var dataModel = new WorkTimeDetaile();
            int WhetherToWork = 0;
            int IsFlexibleWork = 0;
            if (admin != null)
            {
                if (admin.IsPersonalTime)
                {
                    //个人时间 （没有公休假）
                    if (admin.WorkTime != null)
                    {
                        int workTimeId = admin.WorkTime.Id;
                        int currentYear = DateTime.Now.Year;
                        int currentMonth = DateTime.Now.Month;
                        int workTimrId = admin.WorkTime.Id;
                        var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
                        if (monthList.Count == 2)
                        {
                            int minMonth = monthList.Min();
                            int minYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == minMonth).Year;
                            int maxMonth = monthList.Max();
                            int maxYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == maxMonth).Year;
                            //if (monthList.Contains(currentMonth))
                            //{
                            if ((minMonth == currentMonth && minYear == currentYear) || (maxMonth == currentMonth && maxYear == currentYear))
                            {
                                //if (monthList.Contains(12) && monthList.Contains(1))
                                //{
                                //    month = 12;
                                //    currentYear = currentYear - 1;
                                //}
                                if (minYear > maxYear)
                                {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                                    if (maxMonth == currentMonth)
                                    {//若当月为12月，则下月为一月份
                                        currentMonth = minMonth;
                                        currentYear = minYear;
                                    }
                                    else
                                    {//若当月为1月，且未进行下个月的排班，则下月用最小月（此处即maxMonth）排班
                                        currentMonth = maxMonth;
                                        currentYear = maxYear;
                                    }
                                }
                                else
                                {
                                    if (currentMonth > maxMonth)
                                    {
                                        currentMonth = maxMonth;
                                    }
                                    else
                                    {
                                        currentMonth = minMonth;
                                    }
                                }
                            }
                            else
                            {
                                if (minYear > maxMonth)
                                {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                                    currentMonth = maxMonth;
                                    currentYear = maxYear;
                                }
                                else
                                {
                                    currentMonth = minMonth;
                                    currentYear = minYear;
                                }
                            }
                        }
                        dataModel = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimeId &&
                   x.Year == currentYear && x.Month == currentMonth && x.WorkDay == DateTime.Now.Day);
                        if (dataModel != null)
                        {
                            if (dataModel.WorkTimeType != 2)
                            {
                                msgType = 1;
                                WhetherToWork = 1;
                            }
                        }
                        else
                        {
                            msgType = 2;
                            msg = "今天没有排班信息";
                        }
                    }
                    else
                    {
                        msgType = 3;
                        msg = "当前没有排班时间表";
                    }
                }
                else
                {
                    //职位时间
                    if (admin.JobPosition != null && admin.JobPosition.WorkTime != null)
                    {
                        int week = (int)DateTime.Now.DayOfWeek;
                        if (admin.JobPosition.WorkTime.WorkWeek.Contains(week.ToString()))
                        {
                            WhetherToWork = 1;
                            if (CheckIsHoliday(0))
                            {
                                WhetherToWork = 0;
                            }
                            else
                            {
                                dataModel.AmStartTime = admin.JobPosition.WorkTime.AmStartTime;
                                dataModel.AmEndTime = admin.JobPosition.WorkTime.AmEndTime;
                                dataModel.PmStartTime = admin.JobPosition.WorkTime.PmStartTime;
                                dataModel.PmEndTime = admin.JobPosition.WorkTime.PmEndTime;
                                msgType = 1;
                            }
                            if (admin.JobPosition.WorkTime.IsFlexibleWork)
                            {
                                dataModel.AmStartTime = "";
                                dataModel.AmEndTime = "";
                                dataModel.PmStartTime = "";
                                dataModel.PmEndTime = "";
                                IsFlexibleWork = 1;
                            }

                        }
                        else
                        {
                            WhetherToWork = 0;
                            if (CheckIsHoliday(1))
                            {
                                WhetherToWork = 1;
                                msgType = 5;
                            }
                        }
                    }
                }
            }
            else
            {
                msg = "当前用户不存在";
            }
            var da = new
            {
                msg = msg,
                WhetherToWork = WhetherToWork,
                IsFlexibleWork = IsFlexibleWork,
                AmStartTime = string.IsNullOrEmpty(dataModel.AmStartTime) ? "" : dataModel.AmStartTime,
                AmEndTime = string.IsNullOrEmpty(dataModel.AmEndTime) ? "" : dataModel.AmEndTime,
                PmStartTime = string.IsNullOrEmpty(dataModel.PmStartTime) ? "" : dataModel.PmStartTime,
                PmEndTime = string.IsNullOrEmpty(dataModel.PmEndTime) ? "" : dataModel.PmEndTime
            };
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", da), JsonRequestBehavior.AllowGet);
        }
        public bool CheckIsHoliday(int type)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            var holideyList = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled);
            DateTime currentDate = DateTime.Parse(DateTime.Now.ToShortDateString());
            bool restult = false;
            if (type == 0)
            {
                foreach (var item in holideyList)
                {
                    if (DateTime.Compare(currentDate, DateTime.Parse(item.StartTime.ToShortDateString())) >= 0 &&
                       DateTime.Compare(DateTime.Parse(item.EndTime.ToShortDateString()), currentDate) >= 0)
                    {
                        restult = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (var item in holideyList)
                {
                    var workDates = item.WorkDates;
                    if (!string.IsNullOrEmpty(workDates))
                    {
                        if (workDates.Contains(DateTime.Now.Date.ToString("yyyy/MM/dd")))
                        {
                            restult = true;
                            break;
                        }
                    }
                }
            }
            return restult;
        }

        //获取当月的工作时间 使用职位时间只返回假期
        public JsonResult CheckWorkTime(int adminId, string updateTime)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"), JsonRequestBehavior.AllowGet);
            }
            int WhetherToWork = 0;
            int IsPersonalTime = 0;
            if (admin.IsPersonalTime)
            {
                //使用个人时间
                if (admin.WorkTime == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "没有排班信息"), JsonRequestBehavior.AllowGet);
                }
                int workTimeId = admin.WorkTime.Id;
                IsPersonalTime = 1;

                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;
                var WtdArry = new List<WorkTimeDetaile>();
                var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTimeId).GroupBy(x => x.Month).Select(x => x.Key).ToList();

                int minMonth = monthList.Min();
                int minYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Month == minMonth).Year;
                int maxMonth = monthList.Max();
                int maxYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Month == maxMonth).Year;

                if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                {
                    if (minYear > maxYear)
                    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                        currentMonth = maxMonth;
                        currentYear = maxYear;
                    }
                    else
                    {
                        currentMonth = minMonth;
                        currentYear = minYear;
                    }
                }

                //var _updateDateTime = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month
                //&& x.WorkTimeId == workTimeId).Max(x => x.UpdatedTime);
                var _updateDateTime = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.Year == currentYear && x.Month == currentMonth
                && x.WorkTimeId == workTimeId).Max(x => x.UpdatedTime);
                if (updateTime != _updateDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                {
                    WhetherToWork = 1;
                    //  var modle_list = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month
                    //&& x.WorkTimeId == workTimeId).ToList().Select(x => new
                    //{
                    //    x.WorkDay,
                    //    x.WorkHour,
                    //    x.WorkTimeId,
                    //    x.WorkTimeType,
                    //    x.Year,
                    //    x.Month,
                    //    x.AmStartTime,
                    //    x.PmEndTime,
                    //    UpdatedTime = x.UpdatedTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                    //}).ToList();
                    var modle_list = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.Year == currentYear && x.Month == currentMonth
                  && x.WorkTimeId == workTimeId).ToList().Select(x => new
                  {
                      x.WorkDay,
                      x.WorkHour,
                      x.WorkTimeId,
                      x.WorkTimeType,
                      x.Year,
                      x.Month,
                      x.AmStartTime,
                      x.PmEndTime,
                      UpdatedTime = x.UpdatedTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                  }).ToList();
                    var da = new
                    {
                        WhetherToWork = WhetherToWork,
                        IsPersonalTime = IsPersonalTime,
                        dataList = modle_list
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", da), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var da = new
                    {
                        WhetherToWork = WhetherToWork,
                        IsPersonalTime = IsPersonalTime,
                        updateTime = _updateDateTime,
                        dataList = ""
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", da), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                //使用职位时间
                var holidayList = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled && ((x.StartTime.Year == DateTime.Now.Year && x.StartTime.Month == DateTime.Now.Month) ||
                  (x.EndTime.Year == DateTime.Now.Year && x.EndTime.Month == DateTime.Now.Month))).ToList().Select(x => new
                  {
                      x.Id,
                      StartTime = x.StartTime.ToString("yyyy-MM-dd"),
                      EndTime = x.EndTime.ToString("yyyy-MM-dd"),
                      x.WorkDates,
                      x.HolidayName
                  });
                var da = new
                {
                    WhetherToWork = WhetherToWork,
                    IsPersonalTime = IsPersonalTime,
                    updateTime = DateTime.Now.Month.ToString(),
                    dataList = holidayList
                };
                return Json(new OperationResult(OperationResultType.Success, "获取成功！", da), JsonRequestBehavior.AllowGet);
            }
        }

        public bool CheckIsHoliday(int type, DateTime current_date)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            var holideyList = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled);
            DateTime currentDate = DateTime.Parse(current_date.ToShortDateString());
            bool restult = false;
            if (type == 0)
            {
                foreach (var item in holideyList)
                {
                    if (DateTime.Compare(currentDate, DateTime.Parse(item.StartTime.ToShortDateString())) >= 0 &&
                       DateTime.Compare(DateTime.Parse(item.EndTime.ToShortDateString()), currentDate) >= 0)
                    {
                        restult = true;
                        break;
                    }
                }
            }
            else
            {
                foreach (var item in holideyList)
                {
                    var workDates = item.WorkDates;
                    if (!string.IsNullOrEmpty(workDates))
                    {
                        if (workDates.Contains(currentDate.ToString("yyyy/MM/dd")))
                        {
                            restult = true;
                            break;
                        }
                    }
                }
            }
            return restult;
        }

        public JsonResult CalculationHourField(DateTime startDate, DateTime endDate, int adminId)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            DateTime currentDt = startDate;
            DateTime _workTimeStar = DateTime.Now;
            DateTime _workTimeEndStsr = DateTime.Now;
            bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            double hour = 0;
            if (startDate.ToShortDateString() != endDate.ToShortDateString())
            {
                oper = new OperationResult(OperationResultType.Error, "时间选择有误，必须选择同一天！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            if (admin.IsPersonalTime)
            {
                if (admin.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                var workDeatile = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
 && x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
                if (workDeatile == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (workDeatile.WorkTimeType != 2)
                    {
                        _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmStartTime);
                        _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmEndTime);
                    }
                    else
                    {
                        oper = new OperationResult(OperationResultType.Error, "选择的时间是休息时间，请重新选择！");
                        return Json(oper, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                isFlexibleWork = admin.JobPosition.WorkTime.IsFlexibleWork;
                _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
            }

            if (CheckIsHoliday(0, currentDt))
            {
                oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是假期时间！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
            if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week))
            {
                if (!CheckIsHoliday(1, currentDt))
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是周末！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
            }
            if (!isFlexibleWork)
            {
                if (startDate.Hour > _workTimeEndStsr.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "外勤开始时间大于下班时间！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                if (endDate.Hour < _workTimeStar.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "外勤结束时间小于上班时间！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                if (startDate.Hour < _workTimeStar.Hour)
                {
                    startDate = _workTimeStar;
                }
                if (endDate > _workTimeEndStsr)
                {
                    endDate = _workTimeEndStsr;
                }
            }
            hour = Math.Round((endDate - startDate).TotalHours, 0);
            oper.Data = hour;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 计算加班时长
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public JsonResult CalculationHourOverTime(DateTime startDate, DateTime endDate, int adminId, decimal restHours)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            DateTime currentDt = startDate;
            DateTime _workTimeStar = DateTime.Now;
            DateTime _workTimeEndStsr = DateTime.Now;
            bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            decimal hour = 0;
            if (startDate.ToShortDateString() != endDate.ToShortDateString())
            {
                oper = new OperationResult(OperationResultType.Error, "时间选择有误，必须选择同一天！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            if (admin.IsPersonalTime)
            {
                if (admin.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                var workDeatile = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
 && x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
                if (workDeatile == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (workDeatile.WorkTimeType != 2)
                    {
                        _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmStartTime);
                        _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmEndTime);
                    }
                }
            }
            else
            {
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                isFlexibleWork = admin.JobPosition.WorkTime.IsFlexibleWork;
                _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);

                if (!CheckIsHoliday(0, currentDt))
                {
                    string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
                    if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week))
                    {
                        if (!CheckIsHoliday(1, currentDt))
                        {
                            _workTimeStar = startDate;
                            _workTimeEndStsr = endDate;
                        }
                        else
                        {
                            if (isFlexibleWork)
                            {
                                oper = new OperationResult(OperationResultType.Error, "添加异常，弹性时间在工作日内不能添加！");
                                return Json(oper, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else
                    {
                        if (isFlexibleWork)
                        {
                            oper = new OperationResult(OperationResultType.Error, "添加异常，弹性时间在工作日内不能添加！");
                            return Json(oper, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    _workTimeStar = startDate;
                    _workTimeEndStsr = endDate;
                }
            }
            if ((endDate.Hour < _workTimeEndStsr.Hour && endDate.Hour > _workTimeStar.Hour) || (startDate.Hour > _workTimeStar.Hour && startDate.Hour < _workTimeEndStsr.Hour))
            {
                oper = new OperationResult(OperationResultType.Error, "选择的时间在工作时间内，请重新选择！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            if (startDate.Hour < _workTimeStar.Hour && endDate.Hour > _workTimeStar.Hour)
            {
                oper = new OperationResult(OperationResultType.Error, "选择的时间在工作时间内，请重新选择！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            hour = Convert.ToDecimal(Math.Round((endDate - startDate).TotalHours, 0.0)) - restHours;
            oper.Data = hour;
            var opera = _memberContract.CheckLeavePointsInfo(hour, adminId, "overtime", "api");
            oper.Other = oper.Data;
            oper.Message = oper.Message;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 计算请假时长
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public JsonResult CalculationHourLeave(DateTime startDate, DateTime endDate, int adminId, int LeaveMethod, decimal restHours = 0, double useAnnualLeaveDay = 0.5, int AmOrPm = -1)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            DateTime currentDt = startDate;
            DateTime _workTimeStar = DateTime.Now;
            DateTime _workTimeEndStsr = DateTime.Now;
            bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            decimal hour = 0;
            if (startDate.ToShortDateString() != endDate.ToShortDateString())
            {
                oper = new OperationResult(OperationResultType.Error, "时间选择有误，必须选择同一天！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            if (admin.IsPersonalTime)
            {
                if (admin.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                var workDeatile = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
 && x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
                if (workDeatile == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (workDeatile.WorkTimeType != 2)
                    {
                        if (AmOrPm == 2)
                        {
                            _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmStartTime);
                        }
                        else
                        {
                            _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmStartTime);
                        }
                        if (AmOrPm == 1)
                        {
                            _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmEndTime);
                        }
                        else
                        {
                            _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmEndTime);
                        }
                        //if (LeaveMethod != 1)
                        //{
                        //    oper = new OperationResult(OperationResultType.Success, "");
                        //    oper.Data = 0;
                        //    return Json(oper);
                        //}
                    }
                    else
                    {
                        oper = new OperationResult(OperationResultType.Error, "选择的时间是休息时间，请重新选择！");
                        return Json(oper, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (admin.JobPosition == null
                    || admin.JobPosition.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                isFlexibleWork = admin.JobPosition.WorkTime.IsFlexibleWork;
                if (AmOrPm == 2)
                {
                    _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
                }
                else
                {
                    _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                }
                if (AmOrPm == 1)
                {
                    _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmEndTime);
                }
                else
                {
                    _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
                }
            }

            if (CheckIsHoliday(0, currentDt))
            {
                oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是假期时间！");
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
            if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week))
            {
                if (!CheckIsHoliday(1, currentDt))
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是周末！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
            }
            if (LeaveMethod != 1)
            {
                //oper = new OperationResult(OperationResultType.Success, "");
                //oper.Data = 0;
                //return Json(oper);
                startDate = _workTimeStar;
                int hours_normalwork = (_workTimeEndStsr - _workTimeStar).Hours;
                endDate = _workTimeStar.AddHours(hours_normalwork * useAnnualLeaveDay);
                if (endDate < startDate)
                {
                    endDate = startDate;
                }
            }
            if (!isFlexibleWork)
            {
                if (startDate.Hour > _workTimeEndStsr.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "请假开始时间大于下班时间！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                if (endDate.Hour < _workTimeStar.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "请假结束时间小于上班时间！");
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                if (startDate.Hour < _workTimeStar.Hour)
                {
                    startDate = _workTimeStar;
                }
                if (endDate > _workTimeEndStsr)
                {
                    endDate = _workTimeEndStsr;
                }
            }
            hour = Convert.ToDecimal((endDate - startDate).TotalHours) - restHours;
            oper.Data = hour;
            oper.Other = _memberContract.CheckLeavePointsInfo(hour, adminId, "leave", "api").Data;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 入职申请
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public JsonResult EntryInsert(string mobilePhone, int adminId, string macAddress, string interviewEvaluation, string BankcardImgPath,
            string IdCardImgPath, string HealthCertificateImgPath, string PhotoImgPath)
        {
            Entry dto = new Entry();
            dto.MacAddress = macAddress;
            dto.InterviewEvaluation = interviewEvaluation;
            dto.BankcardImgPath = BankcardImgPath;
            dto.IdCardImgPath = IdCardImgPath;
            dto.HealthCertificateImgPath = HealthCertificateImgPath;
            dto.PhotoImgPath = PhotoImgPath;
            dto.OperatorId = adminId;
            dto.operationId = adminId;
            dto.EntryTime = DateTime.Now;
            dto.ToExamineResult = 0;
            var member = _memberContract.Members.FirstOrDefault(x => x.MobilePhone == mobilePhone);
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (member != null)
            {

                dto.MemberId = member.Id;
                oper = _entryContract.Insert(dto);
            }
            else
            {
                oper = new OperationResult(OperationResultType.Error, "此手机号不是会员");
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllDepart(int Id = 1)
        {
            var oper = new OperationResult(OperationResultType.Success, "获取成功！");

            if (Id == 0)
            {
                var data = _departContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled && _storeContract.Stores.Count(s => s.DepartmentId == x.Id) > 0).Select(x => new
                {
                    x.Id,
                    x.DepartmentName,
                    DeptPhoto = _storeContract.Stores.Where(s => s.DepartmentId == x.Id).Select(s => s.StorePhoto).FirstOrDefault() ?? ""
                }).ToList();

                oper.Data = data;
            }
            else if (Id == -1)
            {
                var data = _departContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled && _storeContract.Stores.Count(s => s.DepartmentId == x.Id) == 0).Select(x => new
                {
                    x.Id,
                    x.DepartmentName,
                    DeptPhoto = ""
                }).ToList();

                var item = new
                {
                    Id = 0,
                    DepartmentName = "店铺",
                    DeptPhoto = ""
                };
                data.Add(item);

                oper.Data = data;
            }
            else
            {
                var data = _departContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled).Select(x => new
                {
                    x.Id,
                    x.DepartmentName,
                    DeptPhoto = _storeContract.Stores.Where(s => s.DepartmentId == x.Id).Select(s => s.StorePhoto).FirstOrDefault() ?? ""
                }).ToList();

                oper.Data = data;
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DepartInfo(int? departId, int pageIndex = 1, int pageSize = 10)
        {
            var oper = new OperationResult(OperationResultType.Success, "获取成功！");

            if (departId == null || departId == 0)
            {
                int count = _adminContract.Administrators.Count(x => !x.IsDeleted && x.IsEnabled);
                if (count > 0)
                {
                    var memer = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize)
                        .Select(x => new
                        {
                            x.Id,
                            MemberName = x.Member.RealName,
                            UserPhoto = x.Member.UserPhoto,
                            MobilePhone = x.Member.MobilePhone,
                            JobPositionName = x.JobPosition.JobPositionName,
                            Email = x.Member.Email,
                            DepartmentName = x.Department != null ? x.Department.DepartmentName : "",
                            DateofBirth = x.Member.DateofBirth != null ? Convert.ToDateTime(x.Member.DateofBirth).ToString("yyyy-MM-dd") : "",
                            CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd")
                        });
                    oper.Data = memer;
                }
                oper.Other = count;
            }
            else
            {
                int count = _adminContract.Administrators.Count(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departId);
                if (count > 0)
                {
                    var memer = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departId).ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize)
                        .Select(x => new
                        {
                            x.Id,
                            MemberName = x.Member.RealName,
                            UserPhoto = x.Member.UserPhoto,
                            MobilePhone = x.Member.MobilePhone,
                            JobPositionName = x.JobPosition.JobPositionName,
                            Email = x.Member.Email,
                            DepartmentName = x.Department != null ? x.Department.DepartmentName : "",
                            DateofBirth = x.Member.DateofBirth != null ? Convert.ToDateTime(x.Member.DateofBirth).ToString("yyyy-MM-dd") : "",
                            CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd")
                        });
                    oper.Data = memer;
                }
                oper.Other = count;
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIsMember(string mobilePhone)
        {
            string errorMsg = string.Empty;
            var oper = new OperationResult(OperationResultType.Error);
            int type = 0;
            var member = _memberContract.Members.Where(x => x.MobilePhone == mobilePhone).FirstOrDefault();
            if (member != null)
            {
                int adminCount = _adminContract.Administrators.Where(x => x.MemberId == member.Id).Count();
                var entry = _entryContract.Entrys.Where(x => x.MemberId == member.Id).FirstOrDefault();
                if (adminCount == 0)
                {
                    if (entry != null)
                    {
                        if (entry.ToExamineResult == 3)
                        {
                            oper.Message = "此会员已经成为员工";
                        }
                        else
                        {
                            oper.Message = "此会员已经在审核中";
                        }
                    }
                    else
                    {
                        oper = new OperationResult(OperationResultType.Success);
                        var da = new
                        {
                            type = type,
                            Id = member == null ? 0 : member.Id,
                            RealName = member == null ? "" : member.RealName,
                            Gender = member == null ? -1 : member.Gender,
                            errorMsg = errorMsg
                        };
                        oper.Data = da;
                    }
                }
                else
                {
                    oper.Message = "此会员已经成为员工";
                }
            }
            else
            {
                oper.Message = "不存在此会员手机号";
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RestInfo(int adminId)
        {
            var rest = _restContract.Rests.FirstOrDefault(x => x.AdminId == adminId);
            double annualLeaveDays = rest == null ? 0 : rest.AnnualLeaveDays;
            double paidLeaveDays = rest == null ? 0 : rest.PaidLeaveDays;
            return Json(new OperationResult(OperationResultType.Success, "获取成功!", new { annualLeaveDays = annualLeaveDays, paidLeaveDays = paidLeaveDays }));
        }

        //请假记录
        public JsonResult GetLeavList(int adminId)
        {
            var _list = _leaveInfoContract.LeaveInfos.Where(x => x.IsEnabled && !x.IsDeleted && x.AdminId == adminId && x.StartTime != null);
            List<int> _year = _list.Select(x => x.StartTime.Year).Distinct().ToList();
            var list_data = new List<object>();
            foreach (var item in _year)
            {
                var _leave = _list.Where(x => x.StartTime.Year == item).OrderByDescending(x => x.StartTime.Month).AsEnumerable().
                    Select(x => new
                    {
                        x.Id,
                        x.VerifyType,
                        StartTime = x.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        EndTime = x.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        x.LeaveReason
                    }).ToList();
                list_data.Add(new { Year = item, data = _leave });
            }
            var oper = new OperationResult(OperationResultType.Success, "获取成功！");
            oper.Data = list_data;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        //外勤记录
        public JsonResult GetFileList(int adminId)
        {
            var _list = _fieldContract.Fields.Where(x => x.IsEnabled && !x.IsDeleted && x.AdminId == adminId && x.StartTime != null);
            List<int> _year = _list.Select(x => x.StartTime.Year).Distinct().ToList();
            var list_data = new List<object>();
            foreach (var item in _year)
            {
                var _leave = _list.Where(x => x.StartTime.Year == item).OrderByDescending(x => x.StartTime.Month).AsEnumerable().
                    Select(x => new
                    {
                        x.Id,
                        x.VerifyType,
                        StartTime = x.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        EndTime = x.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        x.FieldReason
                    }).ToList();
                list_data.Add(new { Year = item, data = _leave });
            }
            var oper = new OperationResult(OperationResultType.Success, "获取成功！");
            oper.Data = list_data;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        //加班记录
        public JsonResult GetOverTimeList(int adminId)
        {
            var _list = _overtimeContract.Overtimes.Where(x => x.IsEnabled && !x.IsDeleted && x.AdminId == adminId && x.StartTime != null);
            List<int> _year = _list.Select(x => x.StartTime.Year).Distinct().ToList();
            var list_data = new List<object>();
            foreach (var item in _year)
            {
                var _leave = _list.Where(x => x.StartTime.Year == item).OrderByDescending(x => x.StartTime.Month).AsEnumerable().
                    Select(x => new
                    {
                        x.Id,
                        x.VerifyType,
                        StartTime = x.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        EndTime = x.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        x.OvertimeReason
                    }).ToList();
                list_data.Add(new { Year = item, data = _leave });
            }
            var oper = new OperationResult(OperationResultType.Success, "获取成功！");
            oper.Data = list_data;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
    }
}