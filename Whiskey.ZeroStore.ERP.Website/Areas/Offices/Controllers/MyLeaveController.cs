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
using Whiskey.ZeroStore.ERP.Services.InterfaceContracts;
using Whiskey.ZeroStore.ERP.Services.Achieve;
using System.Xml;
using System.Xml.Linq;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    //[License(CheckMode.Verify)]
    public class MyLeaveController : Controller
    {

        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly ILeaveInfoContract _leaveInfoContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IHolidayContract _holidayContract;

        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;

        protected readonly IMemberContract _memberContract;

        public MyLeaveController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            ILeaveInfoContract leaveInfoContract,
            IWorkTimeContract workTimeContract,
            IHolidayContract holidayContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IMemberContract memberContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _leaveInfoContract = leaveInfoContract;
            _workTimeContract = workTimeContract;
            _holidayContract = holidayContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _memberContract = memberContract;
        }
        #endregion

        #region 初始化请假界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            bool isPower = false;
            int count = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.SubordinateId == adminId).Count();
            int index = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.JobPosition.IsLeader == true).Count();
            if (count > 0 || index > 0)
            {
                isPower = true;
            }
            ViewBag.Power = isPower;
            ViewBag.AdminId = adminId;
            return View();
        }
        #endregion

        #region 获取请假数据列表
        public async Task<ActionResult> List()
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
                    x.IsEnabled,
                    x.IsDeleted,
                    x.UseAnnualLeaveDay,
                    x.LeaveMethod
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 员工申请请假

        /// <summary>
        /// 初始化员工请假界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Rest rest = _restContract.Rests.Where(x => x.AdminId == adminId).FirstOrDefault();
            double annualLeaveDays = rest == null ? 0 : rest.AnnualLeaveDays;
            double paidLeaveDays = rest == null ? 0 : rest.PaidLeaveDays;
            ViewBag.MayUse = annualLeaveDays;
            ViewBag.PaidCount = paidLeaveDays;
            return PartialView();
        }

        /// <summary>
        /// 添加员工请假
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(LeaveInfoDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            if (dto.LeaveMethod != 1)
            {
                //dto.EndTime = dto.StartTime;

                if (dto.UseAnnualLeaveDay == 1)
                {
                    dto.AmOrPm = 0;
                }
            }
            else
            {
                dto.AmOrPm = -1;
            }
            OperationResult oper = CalculationHour(dto.StartTime, dto.EndTime, dto.AdminId, dto.LeaveMethod, dto.UseAnnualLeaveDay, dto.AmOrPm);
            if (oper.ResultType != OperationResultType.Success)
            {
                return Json(oper);
            }
            string strDay = oper.Data.ToString();
            if (dto.LeaveMethod == 1)
            {
                dto.UseAnnualLeaveDay = 0;
                dto.LeaveDays = double.Parse(strDay) - dto.RestHours;
            }
            else
            {
                dto.RestHours = 0;
                dto.LeaveDays = double.Parse(strDay);
            }

            if (dto.LeaveMethod == 1)
            {
                oper = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.LeaveDays), -1, "leave");
                if (oper.ResultType != OperationResultType.Success)
                {
                    return Json(oper);
                }
                decimal deductionLeavePoints = 0;
                if (oper.Data != null && decimal.TryParse(oper.Data.ToString(), out deductionLeavePoints))
                {
                    dto.DeductionLeavePoints = deductionLeavePoints;
                    AdministratorDto adm = _administratorContract.Edit(dto.AdminId);
                    int memberId = adm.MemberId ?? 0;
                    //扣除修改后需要的积分
                    oper = _memberContract.ReturnPoints(memberId, -dto.DeductionLeavePoints);

                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return Json(oper);
                    }
                }
            }

            oper = _leaveInfoContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 员工修改审核中或者修和不通过的请假信息
        /// <summary>
        /// 初始化修改请假界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Rest rest = _restContract.Rests.Where(x => x.AdminId == adminId).FirstOrDefault();
            double annualLeaveDays = 0;
            double paidLeaveDays = 0;
            if (rest != null)
            {
                annualLeaveDays = rest.AnnualLeaveDays;
                paidLeaveDays = rest.PaidLeaveDays;
            }
            ViewBag.MayUse = annualLeaveDays;
            ViewBag.PaidCount = paidLeaveDays;
            var entityDto = _leaveInfoContract.Edit(Id);
            return PartialView(entityDto);
        }

        /// <summary>
        /// 修改请假数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(LeaveInfoDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            if (dto.LeaveMethod != 1)
            {
                //dto.EndTime = dto.StartTime;

                if (dto.UseAnnualLeaveDay == 1)
                {
                    dto.AmOrPm = 0;
                }
            }
            else
            {
                dto.AmOrPm = -1;
            }
            OperationResult oper = CalculationHour(dto.StartTime, dto.EndTime, dto.AdminId, dto.LeaveMethod, dto.UseAnnualLeaveDay, dto.AmOrPm);
            if (oper.ResultType != OperationResultType.Success)
            {
                return Json(oper);
            }
            string strDay = oper.Data.ToString();
            if (dto.LeaveMethod == 1)
            {
                dto.UseAnnualLeaveDay = 0;
                dto.LeaveDays = double.Parse(strDay) - dto.RestHours;
            }
            else
            {
                dto.RestHours = 0;
                dto.LeaveDays = double.Parse(strDay);
            }
            if (dto.LeaveMethod == 1)
            {
                AdministratorDto adm = _administratorContract.Edit(dto.AdminId);
                int memberId = adm.MemberId ?? 0;
                MemberDto member = _memberContract.Edit(memberId);
                //返还上次扣除的积分
                oper = _memberContract.ReturnPoints(memberId, dto.DeductionLeavePoints);

                if (oper.ResultType != OperationResultType.Success)
                {
                    return Json(oper);
                }

                oper = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.LeaveDays), -1, "leave");
                if (oper.ResultType != OperationResultType.Success)
                {
                    return Json(oper);
                }
                decimal deductionLeavePoints = 0;
                if (oper.Data != null && decimal.TryParse(oper.Data.ToString(), out deductionLeavePoints))
                {
                    dto.DeductionLeavePoints = deductionLeavePoints;

                    //扣除修改后需要的积分
                    oper = _memberContract.ReturnPoints(memberId, -dto.DeductionLeavePoints);

                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return Json(oper);
                    }
                }
            }
            oper = _leaveInfoContract.Update(dto);
            return Json(oper);
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity = _leaveInfoContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 计算工作天数
        /// <summary>
        /// 计算工作天数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ComputeWorkDay(string StartDateStr, string EndDateStr, int LeaveMethod)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            DateTime StartDate = DateTime.Now;
            DateTime EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(EndDateStr))
            {
                EndDate = DateTime.Parse(EndDateStr);
            }
            if (!string.IsNullOrEmpty(StartDateStr))
            {
                StartDate = DateTime.Parse(StartDateStr);
            }
            OperationResult oper = CalculationHour(StartDate, EndDate, adminId, LeaveMethod);//GetDay
            return Json(oper);
        }
        /// <summary>
        /// 计算请假时长
        /// </summary>
        /// <param name="startDate">开始时间</param>
        /// <param name="endDate">结束时间</param>
        /// <param name="adminId">管理员ID</param>
        /// <param name="LeaveMethod">请假类型</param>
        /// <param name="useAnnualLeaveDay">年假时间（0.5为半天，1为全天）</param>
        /// <param name="AmOrPm">上午还是下午（-1为非年假或带薪休假，0为全天，1为上午，2为下午）</param>
        /// <returns></returns>
        private OperationResult CalculationHour(DateTime startDate, DateTime endDate, int adminId, int LeaveMethod, double useAnnualLeaveDay = 0.5, int AmOrPm = -1)
        {
            var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            DateTime currentDt = startDate;
            DateTime _workTimeStar = DateTime.Now;
            DateTime _workTimeEndStsr = DateTime.Now;
            bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            double hour = 0;
            if (admin.IsPersonalTime)
            {
                var workDeatile = admin.WorkTime == null ? null : _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
      && x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
                if (workDeatile == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return oper;
                }
                if (workDeatile.WorkTimeType == 2)
                {
                    oper = new OperationResult(OperationResultType.Error, "选择的时间是休息时间，请重新选择！");
                    return oper;
                }
                if (AmOrPm == 2)
                {
                    _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
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
                //    return oper;
                //}
            }
            else
            {
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
                    return oper;
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

                string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
                if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week) && !CheckIsHoliday(1, currentDt))
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是周末！");
                    return oper;
                }
            }

            if (CheckIsHoliday(0, currentDt))
            {
                oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是假期时间！");
                return oper;
            }
            if (LeaveMethod != 1)
            {
                //oper = new OperationResult(OperationResultType.Success, "");
                //oper.Data = 0;
                //return oper;
                //startDate = _workTimeStar;
                //int hours_normalwork = (_workTimeEndStsr - _workTimeStar).Hours;
                startDate = _workTimeStar;
                endDate = _workTimeEndStsr;
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
                    return oper;
                }
                if (endDate.Hour < _workTimeStar.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "请假结束时间小于上班时间！");
                    return oper;
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
            hour = (endDate - startDate).TotalHours;
            oper.Data = hour;
            return oper;
        }



        #endregion
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
    }
}