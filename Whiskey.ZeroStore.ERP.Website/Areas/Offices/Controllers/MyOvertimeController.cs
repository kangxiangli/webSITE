using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Xml.Linq;
using Whiskey.Utility;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class MyOvertimeController : Controller
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IOvertimeContract _overtimeContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IHolidayContract _holidayContract;

        protected readonly IDepartmentContract _departmentContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IConfigureContract _configureContract;
        public MyOvertimeController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            IOvertimeContract overtimeContract,
            IWorkTimeContract workTimeContract,
            IHolidayContract holidayContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IMemberContract memberContract,
            IConfigureContract configureContract)
        {
            _administratorContract = administratorContract;
            _attendanceContract = attendanceContract;
            _overtimeContract = overtimeContract;
            _workTimeContract = workTimeContract;
            _departmentContract = departmentContract;
            _holidayContract = holidayContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _memberContract = memberContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 初始化加班界面

        /// <summary>
        /// 初始化加班界面
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

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Overtime, bool>> predicate = FilterHelper.GetExpression<Overtime>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _overtimeContract.Overtimes.Where<Overtime, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.VerifyType,
                    x.OvertimeDays,
                    x.IsEnabled,
                    x.IsDeleted,
                    x.GetPoints
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 初始化添加界面
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            decimal OvertimePoints = 0;
            //decimal.TryParse(XmlStaticHelper.GetXmlNodeByXpath("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            decimal.TryParse(_configureContract.GetConfigureValue("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            ViewBag.OvertimePoints = OvertimePoints;
            return PartialView();
        }
        #endregion

        #region 添加数据
        [HttpPost]
        public JsonResult Create(OvertimeDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = CalculationHour(dto.StartTime, dto.EndTime, dto.AdminId);
            if (oper.ResultType != OperationResultType.Success)
            {
                return Json(oper);
            }
            string strDay = oper.Data.ToString();
            dto.OvertimeDays = double.Parse(strDay) - dto.RestHours;
            OperationResult oper_point = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.OvertimeDays), -1, "overtime");
            dto.GetPoints = Convert.ToDecimal(oper_point.Data);
            oper = _overtimeContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 初始化修改界面
        public ActionResult Update(int Id)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var entityDto = _overtimeContract.Edit(Id);
            decimal OvertimePoints = 0;
            //decimal.TryParse(XmlStaticHelper.GetXmlNodeByXpath("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            decimal.TryParse(_configureContract.GetConfigureValue("LeavePoints", "LeavePointsconfig", "LeavePoints", "0"), out OvertimePoints);
            OperationResult oper = CalculationHour(entityDto.StartTime, entityDto.EndTime, entityDto.AdminId);
            decimal Lengthleave = 0;
            if (oper.ResultType == OperationResultType.Success)
            {
                decimal.TryParse(oper.Data.ToString(), out Lengthleave);
            }
            ViewBag.OvertimePoints = OvertimePoints;
            ViewBag.RestHours = entityDto.RestHours;
            ViewBag.Lengthleave = Lengthleave;
            return PartialView(entityDto);
        }

        #endregion           

        #region 修改数据
        [HttpPost]
        public JsonResult Update(OvertimeDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = CalculationHour(dto.StartTime, dto.EndTime, dto.AdminId);
            if (oper.ResultType == OperationResultType.Success)
            {
                string strDay = oper.Data.ToString();
                dto.OvertimeDays = double.Parse(strDay) - dto.RestHours;
                OperationResult oper_point = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.OvertimeDays), -1, "overtime");
                dto.GetPoints = Convert.ToDecimal(oper_point.Data);
                oper = _overtimeContract.Update(dto);
            }
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
            var entity = _overtimeContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 计算加班工作天数
        /// <summary>
        /// 计算加班工作天数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ComputeWorkDay(DateTime StartDate, DateTime EndDate)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = CalculationHour(StartDate, EndDate, adminId);
            return Json(oper);
        }

        #endregion


        /// <summary>
        /// 计算加班时长
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult CalculationHour(DateTime startDate, DateTime endDate, int adminId)
        {
            var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            DateTime currentDt = startDate;
            DateTime _workTimeStar = DateTime.Now;
            DateTime _workTimeEndStsr = DateTime.Now;
            bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            double hour = 0;
            bool IsWorkDay = true;      //是否工作日
            if (admin.IsPersonalTime)
            {
                var workDeatile = admin.WorkTime == null ? null : _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
       && x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
                if (workDeatile == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
                    return oper;
                }
                if (workDeatile.WorkTimeType != 2)
                {
                    _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmStartTime);
                    _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmEndTime);
                }
                else
                {
                    IsWorkDay = false;
                }
            }
            else
            {
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
                    return oper;
                }
                isFlexibleWork = admin.JobPosition.WorkTime.IsFlexibleWork;
                _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
                if (!CheckIsHoliday(0, currentDt))
                {
                    string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
                    if (admin.JobPosition.WorkTime.WorkWeek.Contains(_week) && isFlexibleWork)
                    {
                        oper = new OperationResult(OperationResultType.Error, "添加异常，弹性时间在工作日内不能添加！");
                        return oper;
                    }
                    else if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week) && isFlexibleWork)
                    {
                        oper = new OperationResult(OperationResultType.Error, "添加异常，弹性时间在工作日内不能添加！");
                        return oper;
                    }
                    else if (!admin.JobPosition.WorkTime.WorkWeek.Contains(_week) && !CheckIsHoliday(1, currentDt))
                    {
                        _workTimeStar = startDate;
                        _workTimeEndStsr = endDate;
                    }
                    IsWorkDay = false;
                }
                else
                {
                    _workTimeStar = startDate;
                    _workTimeEndStsr = endDate;
                }
            }
            if (IsWorkDay)
            {
                if ((endDate.Hour < _workTimeEndStsr.Hour && endDate.Hour > _workTimeStar.Hour) || (startDate.Hour > _workTimeStar.Hour && startDate.Hour < _workTimeEndStsr.Hour))
                {
                    oper = new OperationResult(OperationResultType.Error, "选择的时间在工作时间内，请重新选择！");
                    return oper;
                }
                if (startDate.Hour < _workTimeStar.Hour && endDate.Hour > _workTimeStar.Hour)
                {
                    oper = new OperationResult(OperationResultType.Error, "选择的时间在工作时间内，请重新选择！");
                    return oper;
                }
            }
            hour = (endDate - startDate).TotalHours;
            oper.Data = hour;
            return oper;
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
    }
}