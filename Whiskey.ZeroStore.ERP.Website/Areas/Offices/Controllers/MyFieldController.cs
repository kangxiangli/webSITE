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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    /// <summary>
    /// 员工外勤
    /// </summary>
   // [License(CheckMode.Verify)]
    public class MyFieldController : Controller
    {

        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly IFieldContract _fieldContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IHolidayContract _holidayContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        public MyFieldController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            IFieldContract fieldContract,
            IWorkTimeContract workTimeContract,
            IHolidayContract holidayContract,
            IWorkTimeDetaileContract workTimeDetaileContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _fieldContract = fieldContract;
            _workTimeContract = workTimeContract;
            _holidayContract = holidayContract;
            _workTimeDetaileContract = workTimeDetaileContract;
        }
        #endregion

        #region 初始化外勤界面

        /// <summary>
        /// 初始化外勤界面
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
                    x.VerifyType,
                    x.FieldDays,
                    x.FieldWorkDays,
                    x.IsEnabled,
                    x.IsDeleted,
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
            return PartialView();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(FieldDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = _fieldContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 初始化修改界面
        public ActionResult Update(int Id)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var entityDto = _fieldContract.Edit(Id);
            return PartialView(entityDto);
        }

        #endregion           

        #region 修改数据
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(FieldDto dto)
        {
            dto.AdminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = _fieldContract.Update(dto);
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
            var entity = _fieldContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 计算外勤工作天数
        /// <summary>
        /// 计算外勤工作天数
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
        /// 计算外勤时长
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult CalculationHour(DateTime startDate, DateTime endDate, int adminId)
        {
            //var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            //DateTime currentDt = startDate;
            //DateTime _workTimeStar = DateTime.Now;
            //DateTime _workTimeEndStsr = DateTime.Now;
            //bool isFlexibleWork = false;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            double hour = 0;
            //           if (admin.IsPersonalTime)
            //           {
            ////               var workDeatile = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.Year == currentDt.Year && x.Month == currentDt.Month
            ////&& x.WorkDay == currentDt.Day && x.WorkTimeId == admin.WorkTime.Id);
            ////               if (workDeatile == null)
            ////               {
            ////                   oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前排班信息！");
            ////                   return oper;
            ////               }
            ////               if (workDeatile.WorkTimeType == 2)
            ////               {
            ////                   oper = new OperationResult(OperationResultType.Error, "选择的时间是休息时间，请重新选择！");
            ////                   return oper;
            ////               }
            //               _workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.AmStartTime);
            //               _workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + workDeatile.PmEndTime);
            //           }
            //           else
            //if (!admin.IsPersonalTime)
            //{
            //    if (admin.JobPosition != null || admin.JobPosition.WorkTime != null)
            //    {
            //        //oper = new OperationResult(OperationResultType.Error, "添加异常，没有当前职位工作时间信息！");
            //        //return oper;
            //        isFlexibleWork = admin.JobPosition.WorkTime.IsFlexibleWork;
            //    }
            //    //_workTimeStar = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
            //    //_workTimeEndStsr = DateTime.Parse(currentDt.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
            //}
            //_workTimeStar = startDate;
            //_workTimeEndStsr = startDate;

            //if (CheckIsHoliday(0, currentDt))
            //{
            //    oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是假期时间！");
            //    return oper;
            //}
            //string _week = Convert.ToInt32(startDate.DayOfWeek).ToString();
            //if (admin.JobPosition != null && admin.JobPosition.WorkTime != null && !admin.JobPosition.WorkTime.WorkWeek.Contains(_week))
            //{
            //    if (!CheckIsHoliday(1, currentDt))
            //    {
            //        oper = new OperationResult(OperationResultType.Error, "添加异常，当前选择时间是周末！");
            //        return oper;
            //    }
            //}
            //if (!isFlexibleWork)
            //{
            //    if (startDate.Hour > _workTimeEndStsr.Hour)
            //    {
            //        oper = new OperationResult(OperationResultType.Error, "外勤开始时间大于下班时间！");
            //        return oper;
            //    }
            //    if (endDate.Hour < _workTimeStar.Hour)
            //    {
            //        oper = new OperationResult(OperationResultType.Error, "外勤结束时间小于上班时间！");
            //        return oper;
            //    }
            //    if (startDate.Hour < _workTimeStar.Hour)
            //    {
            //        startDate = _workTimeStar;
            //    }
            //    if (endDate > _workTimeEndStsr)
            //    {
            //        endDate = _workTimeEndStsr;
            //    }
            //}
            if(endDate<=startDate)
            {
                oper = new OperationResult(OperationResultType.Error, "外勤开始时间必须小于外勤结束时间！");
            }
            hour = Math.Round((endDate - startDate).TotalHours, 0);
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