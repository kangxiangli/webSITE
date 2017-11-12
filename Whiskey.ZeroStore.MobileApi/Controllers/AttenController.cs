using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Achieve;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.InterfaceContracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class AttenController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttenController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IHolidayContract _holidayContract;

        protected readonly IAttendanceStatisticsContract _attStatisticsContract;

        protected readonly IOvertimeContract _overtimeContract;

        protected readonly IFieldContract _fieldContract;

        protected readonly ILeaveInfoContract _leaveInfoContract;

        protected readonly IRestContract _restContract;
        private readonly IClassApplicationContract _classApplicationContract;

        protected readonly IAttendanceRepairContract _attendanceRepairContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IConfigureContract _configureContract;
        public AttenController(IAdministratorContract administratorContract,
           IAttendanceContract attendanceContract,
            IMemberContract memberContract,
            IDepartmentContract departmentContract,
            IHolidayContract holidayContract,
            IAttendanceStatisticsContract attStatisticsContract,
            IOvertimeContract overtimeContract,
            IFieldContract fieldContract,
            ILeaveInfoContract leaveInfoContract,
            IRestContract restContract,
            IAttendanceRepairContract attendanceRepairContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IClassApplicationContract classApplicationContract,
            IConfigureContract configureContract)
        {
            _administratorContract = administratorContract;
            _attendanceContract = attendanceContract;
            _memberContract = memberContract;
            _departmentContract = departmentContract;
            _holidayContract = holidayContract;
            _attStatisticsContract = attStatisticsContract;
            _overtimeContract = overtimeContract;
            _fieldContract = fieldContract;
            _leaveInfoContract = leaveInfoContract;
            _restContract = restContract;
            _attendanceRepairContract = attendanceRepairContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _classApplicationContract = classApplicationContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 加班申请
        /// <summary>
        /// 加班申请
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddOvertime(OvertimeDto dto)
        {
            decimal OvertimePoints = 0;
            //decimal.TryParse(XmlStaticHelper.GetXmlNodeByXpath("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            decimal.TryParse(_configureContract.GetConfigureValue("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            OperationResult oper = new OperationResult(OperationResultType.Error, "申请失败");
            OperationResult oper_point = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.OvertimeDays), dto.AdminId);

            dto.GetPoints = Convert.ToDecimal(oper_point.Data);

            oper = _overtimeContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 请假申请
        /// <summary>
        /// 添加员工请假
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddLeaveInfo(LeaveInfoDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                dto.VerifyType = (int)VerifyFlag.Verifing;
                if (dto.LeaveMethod == 1)
                {
                    dto.UseAnnualLeaveDay = 0;
                }
                else
                {
                    dto.RestHours = 0;
                    //dto.LeaveDays = 0;
                }
                oper = _leaveInfoContract.Insert_API(dto);
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后访问";
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// 添加员工请假
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[HttpGet]
        public JsonResult ApiAddLeaveInfo(LeaveInfoDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                dto.VerifyType = (int)VerifyFlag.Verifing;
                if (dto.LeaveMethod == 1)
                {
                    dto.UseAnnualLeaveDay = 0;
                }
                else
                {
                    dto.RestHours = 0;
                    //dto.LeaveDays = 0;
                }
                oper = _leaveInfoContract.Insert_API(dto);
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后访问";
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 外勤申请
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddField(FieldDto dto)
        {
            OperationResult oper = oper = _fieldContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 调班申请
        [HttpPost]
        public JsonResult AddClassApplication(ClassApplication dto)
        {
            if (!_administratorContract.CheckExists(a => a.Id == dto.AdminId && !a.IsDeleted && a.IsEnabled))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"));
            }
            OperationResult oper = _classApplicationContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 调班日期获取
        /// <summary>
        /// 调班日期获取
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetWorkDate(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);

            try
            {
                int AdvanceDay = Convert.ToInt32(GetXmlNodeByXpath_ClassApplication("AdvanceDay"));
                var admin = _administratorContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
                if (admin == null)
                {
                    opera.Message = "用户不存在";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
                List<SelectListItem> workDayList = new List<SelectListItem>();
                int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                if (!admin.IsPersonalTime || (admin.IsPersonalTime && admin.WorkTime == null))
                {
                    opera.Message = "用户不是个人时间";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }

                int currentDay = DateTime.Now.Day + AdvanceDay;
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
                    if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                    {
                        currentMonth = minMonth;
                    }
                    else
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
                }

                var dayArry = _workTimeDetaileContract.WorkTimeDetailes.Where(w => w.IsEnabled && !w.IsDeleted && w.WorkTimeId == admin.WorkTimeId && w.Year == currentYear && w.Month == currentMonth && w.WorkTimeType != 2).Select(w => w.WorkDay);

                if (dayArry == null || dayArry.Count() == 0)
                {
                    opera.Message = "用户暂未安排上班时间";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in dayArry)
                {
                    if (Convert.ToInt32(item) > currentDay && Convert.ToInt32(item) <= days)
                    {
                        workDayList.Add(new SelectListItem() { Value = item.ToString(), Text = item + "号" });
                    }

                }
                opera.ResultType = OperationResultType.Success;
                opera.Data = workDayList;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                opera.Message = "服务器错误";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取调班的间隔日期
        private string GetXmlNodeByXpath_ClassApplication(string xmlFileName)
        {
            //string xpath = Server.MapPath("/Content/Config/SchedulingConfiguration/Scheduling.xml");
            //XmlDocument xmlDoc = new XmlDocument();
            try
            {
                //xmlDoc.Load(xpath); //加载XML文档
                //XmlNode xmlNode = xmlDoc.SelectSingleNode("User").SelectSingleNode(xmlFileName);
                //return xmlNode.InnerText;
                return _configureContract.GetConfigureValue("SchedulingConfiguration", "Scheduling", xmlFileName);
            }
            catch (Exception ex)
            {
                return "0";
                //throw ex; //这里可以定义你自己的异常处理
            }
        }
        #endregion

        #region 补班日期获取
        /// <summary>
        /// 补班日期获取
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetOffday(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);

            try
            {
                int AdvanceDay = Convert.ToInt32(GetXmlNodeByXpath_ClassApplication("AdvanceDay"));
                var admin = _administratorContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
                if (admin == null)
                {
                    opera.Message = "用户不存在";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
                int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

                if (!admin.IsPersonalTime || (admin.IsPersonalTime && admin.WorkTime == null))
                {
                    opera.Message = "用户不是个人时间";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }

                int currentDay = DateTime.Now.Day + AdvanceDay;
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
                    if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                    {
                        currentMonth = minMonth;
                    }
                    else
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
                }

                var dayArryCount = _workTimeDetaileContract.WorkTimeDetailes.Count(w => w.IsEnabled && !w.IsDeleted && w.WorkTimeId == admin.WorkTimeId && w.Year == currentYear && w.Month == currentMonth && w.WorkTimeType != 2);

                if (dayArryCount == 0)
                {
                    opera.Message = "用户暂未安排上班时间";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
                var offDays = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkTimeType == 2 && x.Year == currentYear
&& x.Month == currentMonth)
                    .Select(x => new
                    {
                        x.WorkDay,
                        Name = x.WorkDay + "号"
                    }).ToList();
                opera.ResultType = OperationResultType.Success;
                opera.Data = offDays;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                opera.Message = "服务器错误";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取调班申请列表
        /// <summary>
        /// 获取调班申请列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetClassApplicationList(int adminId, int pageSize, int pageIndex)
        {
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"));
            }
            var count = _classApplicationContract.ClassApplications.Count(x => x.AdminId == adminId && !x.IsDeleted && x.IsEnabled);
            var list = _classApplicationContract.ClassApplications.Where(x => x.AdminId == adminId && !x.IsDeleted && x.IsEnabled).OrderByDescending(c => c.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => new
            {
                x.Id,
                x.Admin.Member.RealName,
                CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                x.Day,
                x.OffDay,
                x.SuccessionDep.DepartmentName,
                UpdatedTime = x.UpdatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                x.ToExamineResult,
                x.IsDeleted,
                SuccessionName = x.Succession.Member.RealName
            });
            OperationResult opera = new OperationResult(OperationResultType.Success, "", list);
            opera.Other = count;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取工作天数
        /// <summary>
        /// 计算加班工作天数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ComputeWorkDay(int AdminId, DateTime StartDate, DateTime EndDate)
        {
            string strStartDate = Request["StartDate"];
            string strEndDate = Request["EndDate"];
            OperationResult oper = GetDay(StartDate, EndDate, AdminId);
            return Json(oper);
        }
        #endregion

        #region 获取加班天数
        public JsonResult GetOvertimeDays(int AdminId, DateTime StartDate, DateTime EndDate)
        {
            OperationResult oper = GetOvertimeDay(StartDate, EndDate, AdminId);
            return Json(oper);
        }
        #endregion

        #region 获取外勤天数
        public JsonResult GetFieldDays(int AdminId, DateTime StartDate, DateTime EndDate)
        {
            OperationResult oper = GetFieldDay(StartDate, EndDate, AdminId);
            return Json(oper);
        }

        #endregion

        #region 获取工作时间
        /// <summary>
        /// 获取工作时间
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult GetWorkTime(DateTime startDate, DateTime endDate, int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            if (admin == null)
            {
                oper.Message = "员工不存在";
                return oper;
            }
            JobPosition jobPos = admin.JobPosition;
            if (jobPos == null)
            {
                oper.Message = "请联系管理员给你分配职位";
                return oper;
            }
            WorkTime workTime = new WorkTime();
            if (admin.WorkTime != null && admin.WorkTime.IsEnabled == true)
            {
                workTime = admin.WorkTime;
            }
            else
            {
                workTime = jobPos.WorkTime;
            }
            oper.ResultType = OperationResultType.Success;
            oper.Data = workTime;
            return oper;
        }
        #endregion

        #region 计算工作天数
        /// <summary>
        /// 计算天数
        /// </summary>
        /// <param name="strStartDate"></param>
        /// <param name="strEndDate"></param>
        /// <returns></returns>
        private OperationResult GetDay(DateTime startDate, DateTime endDate, int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Dictionary<string, int> dic = GetHoliday();
            oper = GetWorkTime(startDate, endDate, adminId);
            if (oper.ResultType == OperationResultType.Success)
            {
                WorkTime workTime = oper.Data as WorkTime;
                //里氏替换
                IDays iworkDays = new WorkDaysOperation();
                oper = iworkDays.ComputeDay(workTime, startDate, endDate, dic);
            }
            return oper;
        }

        #endregion 

        #region 获取假期
        public JsonResult GetVacation(int AdminId)
        {
            int PaidLeaveDays = 0;
            int AnnualLeaveDays = 0;
            Rest rest = _restContract.Rests.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId).FirstOrDefault();
            AttendanceStatistics attenSta = _attStatisticsContract.AttendanceStatisticses.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId).FirstOrDefault();
            if (rest != null)
            {
                AnnualLeaveDays = AnnualLeaveDays + (int)rest.AnnualLeaveDays;
                PaidLeaveDays = PaidLeaveDays + (int)rest.PaidLeaveDays;
            }
            if (attenSta != null)
            {
                PaidLeaveDays = PaidLeaveDays + (int)attenSta.RestDays;
            }
            var data = new
            {
                PaidLeaveDays,
                AnnualLeaveDays
            };
            return Json(data);
        }
        #endregion

        #region 获取休息假期
        private Dictionary<string, int> GetHoliday()
        {
            Dictionary<string, int> dic = _holidayContract.GetHoliday();
            return dic;
        }
        #endregion

        #region 计算加班天数
        /// <summary>
        /// 计算加班天数
        /// </summary>
        /// <param name="dateTime1"></param>
        /// <param name="dateTime2"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private OperationResult GetOvertimeDay(DateTime startDate, DateTime endDate, int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Dictionary<string, int> dic = GetHoliday();
            oper = GetWorkTime(startDate, endDate, adminId);
            if (oper.ResultType == OperationResultType.Success)
            {
                WorkTime workTime = oper.Data as WorkTime;
                //里氏替换
                IDays iworkDays = new OvertimeDaysOperation();
                oper = iworkDays.ComputeDay(workTime, startDate, endDate, dic);
            }
            return oper;
        }
        #endregion

        #region 计算外勤天数
        /// <summary>
        /// 计算外勤天数
        /// </summary>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        private OperationResult GetFieldDay(DateTime startDate, DateTime endDate, int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            //Dictionary<string, int> dic = GetHoliday();
            //oper = GetWorkTime(startDate, endDate, adminId);
            //if (oper.ResultType == OperationResultType.Success)
            //{
            //    WorkTime workTime = oper.Data as WorkTime;
            //    //里氏替换
            IDays iworkDays = new FiledDaysOperation();
            //oper = iworkDays.ComputeDay(workTime, startDate, endDate, dic);
            oper = iworkDays.ComputeDay(null, startDate, endDate, null);
            //}
            return oper;
        }
        #endregion

        #region 根据时期获取考勤数据
        /// <summary>
        /// 根据时期获取考勤数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAttenByDate(int AdminId, DateTime Date)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍候访问");
            try
            {
                Administrator admin = _administratorContract.View(AdminId);
                //获取工作时间
                var WToper = GetWorkTime(admin);
                WorkTime workTime = new WorkTime();
                if (WToper.ResultType != OperationResultType.Success)
                {
                    WToper.Data = null;
                    return Json(WToper, JsonRequestBehavior.AllowGet);
                }
                workTime = WToper.Data as WorkTime;

                List<Attendance> listAtten = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId && x.AttendanceTime.Year == Date.Year && x.AttendanceTime.Month == Date.Month).ToList();
                AppAttenStatistics appAtten = new AppAttenStatistics();
                double num = 0;
                appAtten.NormalCount = listAtten.Count();
                var leaveinfos = _leaveInfoContract.LeaveInfos.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month);
                appAtten.LeaveTimes = leaveinfos == null || leaveinfos.Count() == 0 ? num : leaveinfos.ToList().Sum(x => x.LeaveDays);
                var overtimes = _overtimeContract.Overtimes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month);
                appAtten.OvertimeTimes = overtimes == null || overtimes.Count() == 0 ? num : overtimes.ToList().Sum(x => x.OvertimeDays);
                var fields = _fieldContract.Fields.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month);
                appAtten.FieldTimes = fields == null || fields.Count() == 0 ? num : fields.ToList().Sum(x => x.FieldDays);
                appAtten.RepairCount = listAtten.Where(w => w.AttendanceRepairs != null).SelectMany(s => s.AttendanceRepairs).Where(w => w.VerifyType == (int)VerifyFlag.Pass).Count();

                if (listAtten != null)
                {
                    appAtten.ArrivalEarlyCount = listAtten.Where(x => x.ArrivalEarlyMinutes > 0).Count();
                    appAtten.LateCount = listAtten.Where(x => x.IsLate == -1).Count();
                    appAtten.IsLateConfirmRepair = listAtten.Count(x => x.IsLate == -1 && x.AttendanceRepairs != null && x.AttendanceRepairs.Count(r => r.VerifyType == 3 && r.ApiAttenFlag == (int)ApiAttenFlag.Late && !r.IsDeleted && r.IsEnabled) > 0) > 0;
                    appAtten.LeaveEarlyCount = listAtten.Where(x => x.IsLeaveEarly == -1).Count();
                    appAtten.IsLeaveEarlyConfirmRepair = listAtten.Count(x => x.IsLeaveEarly == -1 && x.AttendanceRepairs != null && x.AttendanceRepairs.Count(r => r.VerifyType == 3 && r.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly && !r.IsDeleted && r.IsEnabled) > 0) > 0;
                    appAtten.NoSignOutCount = listAtten.Where(x => x.IsNoSignOut == -1 && (!x.AttendanceTime.ToShortDateString().Equals(DateTime.Now.ToShortDateString()) || (x.AttendanceTime.ToShortDateString().Equals(DateTime.Now.ToShortDateString()) && DateTime.Now > Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + workTime.PmEndTime)))).Count();
                    appAtten.IsNoSignOutConfirmRepair = listAtten.Count(x => x.IsNoSignOut == -1 && (!x.AttendanceTime.ToShortDateString().Equals(DateTime.Now.ToShortDateString()) || (x.AttendanceTime.ToShortDateString().Equals(DateTime.Now.ToShortDateString()) && DateTime.Now > Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + workTime.PmEndTime))) && x.AttendanceRepairs != null && x.AttendanceRepairs.Count(r => r.VerifyType == 3 && r.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut && !r.IsDeleted && r.IsEnabled) > 0) > 0;
                    appAtten.LeaveLateCount = listAtten.Where(x => x.LeaveLateMinutes > 0).Count();
                }
                int absenceCount = 0;// listAtten.Where(x => x.IsAbsence == true && x.AbsenceType == (int)AttendanceFlag.DayOfAbsence).Count();
                var _oper = _attendanceContract.GetNoLoginCount(admin.Id);
                if (_oper.ResultType == OperationResultType.Success)
                {
                    absenceCount = (_oper.Data as List<AttendanceInfo>).Count();
                }
                appAtten.AbsenceCount = absenceCount;
                DateTime endtime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " 18:00");
                if (admin.WorkTime != null && !string.IsNullOrEmpty(admin.WorkTime.PmEndTime))
                {
                    endtime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + admin.WorkTime.PmEndTime);
                }
                //appAtten.NoSignOutCount = listAtten.Where(x => string.IsNullOrEmpty(x.PmEndTime) && endtime < DateTime.Now).Count();
                AttendanceStatistics attSta = _attStatisticsContract.AttendanceStatisticses.FirstOrDefault(x => x.AdminId == AdminId && x.CreatedTime.Year == Date.Year);
                Rest rest = _restContract.Rests.FirstOrDefault(x => x.AdminId == AdminId);
                if (rest != null)
                {
                    appAtten.AnnualLeave = rest.AnnualLeaveDays;
                    appAtten.PaidVacation = rest.PaidLeaveDays;
                }
                if (attSta != null)
                {
                    appAtten.WorkDays = attSta.AbsenceDays;
                    //appAtten.RestDays = attSta.RestDays;
                }
                oper.Message = string.Empty;
                oper.Data = appAtten;
                oper.ResultType = OperationResultType.Success;
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region 获取工作时间
        private OperationResult GetWorkTime(Administrator admin)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请让管理员添加工作时间");
            WorkTime workTime = new WorkTime();
            if (admin.WorkTime != null && admin.WorkTime.IsEnabled == true)
            {
                workTime = admin.WorkTime;
            }
            else if (admin.JobPosition != null)
            {
                workTime = admin.JobPosition.WorkTime;
            }
            else
            {
                return oper;
            }
            if (workTime == null)
            {
                return oper;
            }
            oper.ResultType = OperationResultType.Success;
            oper.Message = string.Empty;
            oper.Data = workTime;
            return oper;
        }
        #endregion

        #region 获取考勤数据列表
        public JsonResult GetAttenList(int AdminId, int AttenFlag, DateTime Date)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            var admin = _administratorContract.View(AdminId);
            IQueryable<Attendance> listAtten = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId);
            List<Attendance> attens = listAtten.Where(x => x.AttendanceTime.Year == Date.Year && x.AttendanceTime.Month == Date.Month).ToList();
            switch (AttenFlag)
            {
                case (int)ApiAttenFlag.Leave:
                    oper.Data = GetLeaveList(AdminId, Date);
                    break;
                case (int)ApiAttenFlag.Overtime:
                    oper.Data = GetOvertime(AdminId, Date);
                    break;
                case (int)ApiAttenFlag.Field:
                    oper.Data = GetFieldList(AdminId, Date);
                    break;
                case (int)ApiAttenFlag.Repair:
                    oper.Data = GetRepair(AdminId, attens, Date);
                    break;
                case (int)ApiAttenFlag.Absence:
                    oper.Data = GetAbsenceList(AdminId, attens, admin);
                    break;
                case (int)ApiAttenFlag.NoSignOut:
                    oper.Data = GetNoSignOut(AdminId, attens, admin);
                    break;
                case (int)ApiAttenFlag.Late:
                    oper.Data = GetLateList(AdminId, attens, admin);
                    break;
                case (int)ApiAttenFlag.LeaveEarly:
                    oper.Data = GetLeaveEarlyList(AdminId, attens, admin);
                    break;
                case (int)ApiAttenFlag.ArrivalEarly:
                    oper.Data = GetArrivalEarlyList(AdminId, attens);
                    break;
                case (int)ApiAttenFlag.LeaveLate:
                    oper.Data = GetLeaveLate(AdminId, attens);
                    break;
                default:
                    oper.Message = "获取数据异常";
                    oper.ResultType = OperationResultType.Error;
                    break;
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取未签到数据
        private object GetAbsenceList(int AdminId, List<Attendance> attens, Administrator admin)
        {
            //var data = (from atten in attens.Where(w => !w.IsPardon).Where(x => x.IsAbsence == true && x.AbsenceType == (int)AttendanceFlag.DayOfAbsence)
            var oper = _attendanceContract.GetNoLoginCount(admin.Id);
            if (oper.ResultType == OperationResultType.Success)
            {
                var attensInfo = _attendanceContract.GetNoLoginCount(admin.Id).Data as List<AttendanceInfo>;
                //由于未签到 考勤记录是空值  所以一下使用默认值 后续可能会变动
                var data = attensInfo.Select(x => new
                {
                    x.Id,
                    AttendanceTime = x.Date,
                    IsPardon = false,
                    Type = x.Id > 0 ? 0 : -1,
                    PaidScore = 0,
                    AdminScore = (admin?.Member?.Score) ?? 0,
                    Week = DateHelper.GetWeekOfZ(Convert.ToDateTime(x.Date))
                });
                return data;
            }
            else
            {
                return null;
            }

        }
        #endregion

        #region 获取早到数据
        private object GetArrivalEarlyList(int AdminId, List<Attendance> attens)
        {
            attens = attens.Where(x => x.ArrivalEarlyMinutes > 0).ToList();
            var data = attens.Select(x => new
            {
                x.Id,
                AttendanceTime = x.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                x.ArrivalEarlyMinutes,
                Week = DateHelper.GetWeekOfZ(x.AttendanceTime)
            }).ToList();
            return data;
        }
        #endregion

        #region 获取外勤数据
        private object GetFieldList(int AdminId, DateTime Date)
        {
            List<Field> list = _fieldContract.Fields.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month).ToList();
            var data = list.OrderByDescending(x => x.CreatedTime).Select(x => new
            {
                x.Id,
                StartTime = x.StartTime.ToString("yyyy-MM-dd hh:mm"),
                EndTime = x.EndTime.ToString("yyyy-MM-dd hh:mm"),
                x.VerifyType,
                Days = x.FieldDays,
                Week = DateHelper.GetWeekOfZ(x.StartTime)
            }).ToList();
            return data;
        }
        #endregion

        #region 获取迟到数据
        private object GetLateList(int AdminId, List<Attendance> attens, Administrator admin)
        {
            var data = (from atten in attens.Where(x => x.IsLate != 0)
                        let rep = atten.AttendanceRepairs.FirstOrDefault(f => f.ApiAttenFlag == (int)ApiAttenFlag.Late)
                        select new
                        {
                            atten.Id,
                            AttendanceTime = atten.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                            Minutes = atten.LateMinutes,
                            IsPardon = rep != null ? rep.IsPardon : false,
                            Type = rep != null ? rep.VerifyType : -1,
                            PaidScore = rep != null ? rep.PaidScore : 0,
                            AdminScore = (admin?.Member?.Score) ?? 0,
                            Week = DateHelper.GetWeekOfZ(atten.AttendanceTime)
                        }).ToList();
            return data;
        }
        #endregion

        #region 获取请假数据
        private object GetLeaveList(int AdminId, DateTime Date)
        {
            List<LeaveInfo> list = _leaveInfoContract.LeaveInfos.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month).ToList();

            var data = list.OrderByDescending(x => x.CreatedTime).Select(x => new
            {
                x.Id,
                StartTime = x.StartTime.ToString("yyyy/MM/dd hh:mm"),
                EndTime = x.EndTime.ToString("yyyy/MM/dd hh:mm"),
                x.VerifyType,
                Days = x.LeaveDays,
                Week = DateHelper.GetWeekOfZ(x.StartTime)
            }).ToList();
            return data;
        }
        #endregion

        #region 获取早退数据
        private object GetLeaveEarlyList(int AdminId, List<Attendance> attens, Administrator admin)
        {
            var data = (from atten in attens.Where(x => x.IsLeaveEarly != 0)
                        let rep = atten.AttendanceRepairs.FirstOrDefault(f => f.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly)
                        select new
                        {
                            atten.Id,
                            AttendanceTime = atten.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                            Minutes = atten.LeaveEarlyMinutes,
                            IsPardon = rep != null ? rep.IsPardon : false,
                            Type = rep != null ? rep.VerifyType : -1,
                            PaidScore = rep != null ? rep.PaidScore : 0,
                            AdminScore = (admin?.Member?.Score) ?? 0,
                            Week = DateHelper.GetWeekOfZ(atten.AttendanceTime)
                        }).ToList();
            return data;
        }
        #endregion

        #region 获取晚退数据
        private object GetLeaveLate(int AdminId, List<Attendance> attens)
        {
            var data = attens.Where(x => x.LeaveLateMinutes > 0).Select(x => new
            {
                x.Id,
                AttendanceTime = x.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                x.LeaveLateMinutes,
                Week = DateHelper.GetWeekOfZ(x.AttendanceTime)
            }).ToList();
            return data;
        }
        #endregion

        #region 获取没有签退数据
        private object GetNoSignOut(int AdminId, List<Attendance> attens, Administrator admin)
        {
            DateTime endtime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " 18:00");
            if (admin.WorkTime != null && admin.WorkTime.PmEndTime != null)
            {
                endtime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + admin.WorkTime.PmEndTime);
            }
            string nowDate = DateTime.Now.ToShortDateString();
            var data = (from atten in attens.Where(x => x.IsNoSignOut != 0 && (!x.AttendanceTime.ToShortDateString().Equals(nowDate) || (x.AttendanceTime.ToShortDateString().Equals(nowDate) && endtime < DateTime.Now)))
                        let rep = atten.AttendanceRepairs.FirstOrDefault(f => f.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut)
                        select new
                        {
                            atten.Id,
                            AttendanceTime = atten.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                            IsPardon = rep != null ? rep.IsPardon : false,
                            Type = rep != null ? rep.VerifyType : -1,
                            PaidScore = rep != null ? rep.PaidScore : 0,
                            AdminScore = (admin?.Member?.Score) ?? 0,
                            Week = DateHelper.GetWeekOfZ(atten.AttendanceTime)
                        }).ToList();
            return data;
        }
        #endregion

        #region 获取加班数据
        private object GetOvertime(int AdminId, DateTime Date)
        {
            List<Overtime> list = _overtimeContract.Overtimes.Where(x => x.AdminId == AdminId && x.StartTime.Year == Date.Year && x.StartTime.Month == Date.Month && x.IsDeleted == false && x.IsEnabled == true).ToList();

            var data = list.OrderByDescending(x => x.CreatedTime).Select(x => new
            {
                x.Id,
                StartTime = x.StartTime.ToString("yyyy-MM-dd hh:mm"),
                EndTime = x.EndTime.ToString("yyyy-MM-dd hh:mm"),
                x.VerifyType,
                Days = x.OvertimeDays,
                Week = DateHelper.GetWeekOfZ(x.StartTime)
            }).ToList();
            return data;
        }
        #endregion

        #region 获取补卡数据
        private object GetRepair(int AdminId, List<Attendance> attens, DateTime Date)
        {
            var dataReps = _attendanceRepairContract.AttendanceRepairs.Where(w => !w.IsDeleted && w.IsEnabled && w.AdminId == AdminId && w.AttendanceId != null)
                            .Where(w => w.Attendance.AttendanceTime.Year == Date.Year && w.Attendance.AttendanceTime.Month == Date.Month)
                            .ToList();
            var data = (from x in dataReps.Where(w => w.VerifyType == (int)VerifyFlag.Pass)
                        select new
                        {
                            x.Id,
                            AttendanceTime = x.Attendance.AttendanceTime.ToString("yyyy-MM-dd hh:mm"),
                            Type = x.ApiAttenFlag,
                            IsPardon = true,
                            Week = DateHelper.GetWeekOfZ(x.Attendance.AttendanceTime)
                        }).ToList();
            return data;
        }
        #endregion

        #region 补卡
        [HttpPost]
        public JsonResult ApplyRepair(int AdminId, int Id, int? AttenFlag, string Reasons)
        {
            OperationResult oper = _attendanceRepairContract.ApplyRepair(AdminId, Id, AttenFlag, Reasons);
            return Json(oper);
        }
        #endregion

        #region 确认补卡
        public JsonResult ConfirmRepair(int AdminId, int Id, int? AttenFlag)
        {
            OperationResult oper = _attendanceRepairContract.ConfirmRepair(AdminId, Id, AttenFlag);
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApplyNoLoginRepair(int adminId, string nowTime, int? AttenFlag)
        {
            OperationResult oper = _attendanceRepairContract.ApplyNoLoginRepair(adminId, nowTime, AttenFlag);
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region 系统自动生成确认扣除积分补卡记录(扣除双倍积分)
        [HttpPost]
        public JsonResult ApplyRepairBySystem(int adminId)
        {
            var oper = _attendanceRepairContract.ApplyRepairBySystem(adminId, "api");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 系统确认补卡
        [HttpPost]
        public JsonResult ConfirmRepairBySystem(int AdminId)
        {
            OperationResult oper = _attendanceRepairContract.ConfirmRepairBySystem(AdminId, "api");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }

    #region APP考勤
    /// <summary>
    /// APP考勤
    /// </summary>
    public class AppAttenStatistics
    {
        /// <summary>
        /// 正常上班天数
        /// </summary>
        public int NormalCount { get; set; }

        /// <summary>
        /// 请假时长
        /// </summary>
        public double LeaveTimes { get; set; }

        /// <summary>
        /// 加班时长
        /// </summary>
        public double OvertimeTimes { get; set; }

        /// <summary>
        /// 外勤时长
        /// </summary>
        public double FieldTimes { get; set; }

        /// <summary>
        /// 补卡次数
        /// </summary>
        public int RepairCount { get; set; }

        /// <summary>
        /// 缺勤次数
        /// </summary>
        public int AbsenceCount { get; set; }

        /// <summary>
        /// 未签退次数
        /// </summary>
        public int NoSignOutCount { get; set; }

        /// <summary>
        /// 是否有未签退需要确认补卡
        /// </summary>
        public bool IsNoSignOutConfirmRepair { get; set; }

        /// <summary>
        /// 迟到次数
        /// </summary>
        public int LateCount { get; set; }

        /// <summary>
        /// 是否有迟到需要确认补卡
        /// </summary>
        public bool IsLateConfirmRepair { get; set; }


        /// <summary>
        /// 早到次数
        /// </summary>
        public double ArrivalEarlyCount { get; set; }

        /// <summary>
        /// 晚退次数
        /// </summary>
        public double LeaveLateCount { get; set; }

        /// <summary>
        /// 早退次数
        /// </summary>
        public int LeaveEarlyCount { get; set; }

        /// <summary>
        /// 是否有早退需要确认补卡
        /// </summary>
        public bool IsLeaveEarlyConfirmRepair { get; set; }

        /// <summary>
        /// 补班
        /// </summary>
        public double WorkDays { get; set; }

        /// <summary>
        /// 年假
        /// </summary>
        public double AnnualLeave { get; set; }

        /// <summary>
        /// 带薪休假
        /// </summary>
        public double PaidVacation { get; set; }
    }
    #endregion
}