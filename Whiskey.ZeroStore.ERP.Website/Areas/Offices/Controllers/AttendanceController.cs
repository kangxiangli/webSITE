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
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class AttendanceController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IAdministratorContract _adminContract;

        protected readonly IHolidayContract _holidayContract;
        protected readonly IOvertimeContract _overtimeContract;
        protected readonly ILeaveInfoContract _leaveInfoContract;
        protected readonly IFieldContract _fieldContract;
        public AttendanceController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IAdministratorContract adminContract,
            IHolidayContract holidayContract,
            IOvertimeContract overtimeContract,
            ILeaveInfoContract leaveInfoContract,
            IFieldContract fieldContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _adminContract = adminContract;
            _holidayContract = holidayContract;
            _overtimeContract = overtimeContract;
            _leaveInfoContract = leaveInfoContract;
            _fieldContract = fieldContract;
        }
        #endregion

        #region 初始化界面

        [Layout]
        public ActionResult Index()
        {

            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(GetCurUserDepartList());
            ViewBag.depList = list;
            return View();
        }
        #endregion

        public List<SelectListItem> GetCurUserDepartList()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var admin = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId).FirstOrDefault();
            List<SelectListItem> list = new List<SelectListItem>();
            var listDep = admin?.JobPosition?.Departments;
            if (listDep.IsNotNullOrEmptyThis())
            {
                list.AddRange(listDep.Select(s => new SelectListItem()
                {
                    Value = s.Id.ToString(),
                    Text = s.DepartmentName
                }));
            }

            if (admin.JobPosition != null)
            {
                //判断是否是领导
                if (admin.JobPosition.IsLeader)
                {
                    var departmentId = admin.JobPosition.DepartmentId;
                    var name = admin.JobPosition.Department.DepartmentName;
                    if (!list.Exists(x => x.Value == admin.JobPosition.DepartmentId.ToString()))
                    {
                        list.Insert(0, new SelectListItem()
                        {
                            Value = departmentId.ToString(),
                            Text = name
                        });
                    }
                }
            }

            return list;
        }

        #region 获取数据列表

        public async Task<ActionResult> List(int? SelDepartmentId)
        {
            var listDeps = GetCurUserDepartList().Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            GridRequest request = new GridRequest(Request);
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(predicate);
                string startDate = Request["startDate"];
                string endDate = Request["endDate"];
                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                if (SelDepartmentId.HasValue)
                {
                    listDeps = listDeps.Where(x => x == SelDepartmentId.Value).ToList();
                }
                var da = listDeps.OrderBy(x => x).Skip(request.PageCondition.PageIndex)
                   .Take(request.PageCondition.PageSize).Select(x => x).ToList();

                listAttendance = from a in listAttendance
                                 where da.Contains(a.Administrator.DepartmentId.Value)
                                 select a;
                var admin_list = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && da.Contains(x.DepartmentId.Value));
                //取出考勤年份
                List<int> listYear = new List<int>();
                if (!string.IsNullOrEmpty(endDate) && !string.IsNullOrEmpty(startDate))
                {
                    listYear.Add(DateTime.Parse(startDate).Year);
                    listYear.Add(DateTime.Parse(endDate).Year);
                    listYear = listYear.Distinct().ToList();
                }
                else
                {
                    listYear = listAttendance.Select(x => x.AttendanceTime.Year).Distinct().OrderByDescending(x => x).ToList();
                }
                List<AttendanceSeach> listAttendanceInfo = new List<AttendanceSeach>();
                int id = 1;
                foreach (int year in listYear)
                {
                    List<int> monthList = listAttendance.Where(x => x.AttendanceTime.Year == year).Select(x => x.AttendanceTime.Month).Distinct().OrderByDescending(x => x).ToList();
                    foreach (var i in monthList)
                    {
                        count++;
                        id++;
                        List<Attendance> listAtten = listAttendance.Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == i).ToList();
                        if (listAtten.Count > 0)
                        {
                            //整天请假 
                            var leaveAllDaysList = from a in _leaveInfoContract.LeaveInfos
                                                   join b in admin_list on a.AdminId equals b.Id
                                                   where !a.IsDeleted && a.IsEnabled && a.VerifyType == 1
                                                   group b by new { b.DepartmentId, b.Department.DepartmentName } into g
                                                   select new
                                                   {
                                                       DepartmentId = g.Key.DepartmentId,
                                                       DepartmentName = g.Key.DepartmentName,
                                                       leaveAllDays = (double)g.Count()
                                                   };
                            //整天缺勤
                            var absenceAllDaysList = from a in _fieldContract.Fields
                                                     join b in admin_list on a.AdminId equals b.Id
                                                     where !a.IsDeleted && a.IsEnabled && a.VerifyType == 1
                                                     group b by new { b.DepartmentId, b.Department.DepartmentName } into g
                                                     select new
                                                     {
                                                         DepartmentId = g.Key.DepartmentId,
                                                         DepartmentName = g.Key.DepartmentName,
                                                         leaveAllDays = (double)g.Count()
                                                     };
                            //加班
                            var overtimeList = from a in _overtimeContract.Overtimes
                                               join b in admin_list on a.AdminId equals b.Id
                                               where !a.IsDeleted && a.IsEnabled && a.VerifyType == 1
                                               group b by new { b.DepartmentId, b.Department.DepartmentName } into g
                                               select new
                                               {
                                                   DepartmentId = g.Key.DepartmentId,
                                                   DepartmentName = g.Key.DepartmentName,
                                                   leaveAllDays = (double)g.Count()
                                               };
                            foreach (var item in da)
                            {

                                double LeaveDays = leaveAllDaysList.Where(x => x.DepartmentId == item).Select(x => x.leaveAllDays).FirstOrDefault();
                                double absenceAllDays = absenceAllDaysList.Where(x => x.DepartmentId == item).Select(x => x.leaveAllDays).FirstOrDefault();
                                double overtimeAllDays = overtimeList.Where(x => x.DepartmentId == item).Select(x => x.leaveAllDays).FirstOrDefault();
                                AttendanceSeach attenInfo = new AttendanceSeach()
                                {
                                    Id = "child" + id.ToString(),
                                    Date = year.ToString() + "年" + i.ToString() + "月",
                                    LateCount = listAtten.Where(x => x.IsLate == -1 && x.Administrator.DepartmentId == item).Count(),
                                    LeaveEarlyCount = listAtten.Count(x => x.IsLeaveEarly == -1 && x.Administrator.DepartmentId == item),
                                    NoSignOutCount = listAtten.Count(x => x.IsNoSignOut == -1 && x.Administrator.DepartmentId == item),
                                    LeaveDays = LeaveDays,
                                    AbsenceDays = absenceAllDays,
                                    OverTimeDays = overtimeAllDays,
                                    DepartmentId = item,
                                    DepartmentName = _departmentContract.Departments.Where(x => x.Id == item).Select(x => x.DepartmentName).FirstOrDefault(),
                                    ParentId = "par" + item,
                                    orderId = id
                                };
                                listAttendanceInfo.Add(attenInfo);
                                id++;
                            }
                        }

                    }
                }
                List<AttendanceSeach> dataSouce = new List<AttendanceSeach>();
                foreach (var key in da)
                {
                    var seachM = new AttendanceSeach()
                    {
                        Id = "par" + key,
                        Date = "",
                        LateCount = 0,
                        LeaveEarlyCount = 0,
                        NoSignOutCount = 0,
                        LeaveDays = 0,
                        OverTimeDays = 0,
                        AbsenceDays = 0,
                        DepartmentName = _departmentContract.Departments.Where(c => c.Id == key).Select(c => c.DepartmentName).FirstOrDefault(),
                        DepartmentId = 0,
                        ParentId = ""
                    };
                    dataSouce.Add(seachM);
                    dataSouce.AddRange(listAttendanceInfo.Where(x => x.DepartmentId == key).OrderBy(x => x.orderId).ToList());
                }
                count = listDeps.Count();
                return new GridData<object>(dataSouce, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region 获取数据详情
        public ActionResult Detail()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }

        public int GetWorkDays(Administrator admin, DateTime atteTime, int currentDays)
        {
            string strWeek = admin != null && admin.JobPosition != null && admin.JobPosition.WorkTime != null ? admin.JobPosition.WorkTime.WorkWeek : "";
            if (admin.IsPersonalTime)
            {
                strWeek = admin.WorkTime.WorkWeek;
            }
            string[] arrWeek = strWeek.Split(',');
            List<int> listWeek = new List<int>();
            foreach (string week in arrWeek)
            {
                if (!string.IsNullOrEmpty(week))
                {
                    listWeek.Add(int.Parse(week));
                }
            }
            int workDays = 0;
            for (int i = 0; i < currentDays; i++)
            {
                DateTime dt = atteTime.AddDays(i);
                //先判断是否享有公休假
                if (admin.JobPosition.WorkTime.IsVacations)
                {
                    //判断是不是在公休假时间内
                    int IsHoliday = _holidayContract.Holidays.Where(x => x.IsEnabled && !x.IsDeleted && x.StartTime <= dt && dt <= x.EndTime).Count();
                    if (IsHoliday == 0)
                    {
                        //不再公休假内
                        if (!admin.IsPersonalTime)
                        {
                            int week = (int)dt.DayOfWeek;
                            if (listWeek.Contains(week))
                            {
                                //按照正常工作周计算
                                workDays += 1;
                            }
                            else
                            {
                                //计算是不是需要补班
                                string dtDate = dt.ToString("yyyy/MM/dd");
                                var MakeUpClass = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled && !string.IsNullOrEmpty(x.WorkDates)).ToList();
                                int IsMakeUpClass = (from a in MakeUpClass where a.WorkDates.Contains(dtDate) select a).Count();
                                if (IsMakeUpClass > 0)
                                {
                                    workDays += 1;
                                }
                            }
                        }
                        else
                        {
                            int week = dt.Day;
                            if (listWeek.Contains(week))
                            {
                                //按照正常工作周计算
                                workDays += 1;
                            }
                            else
                            {
                                //计算是不是需要补班
                                string dtDate = dt.ToString("yyyy/MM/dd");
                                var MakeUpClass = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled && !string.IsNullOrEmpty(x.WorkDates)).ToList();
                                int IsMakeUpClass = (from a in MakeUpClass where a.WorkDates.Contains(dtDate) select a).Count();
                                if (IsMakeUpClass > 0)
                                {
                                    workDays += 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //不享有公休假
                    if (!admin.IsPersonalTime)
                    {
                        int week = (int)dt.DayOfWeek;
                        if (listWeek.Contains(week))
                        {
                            //按照正常工作周计算
                            workDays += 1;
                        }
                    }
                    else
                    {
                        int week = (int)dt.Day;
                        if (listWeek.Contains(week))
                        {
                            //按照正常工作周计算
                            workDays += 1;
                        }
                    }
                }

            }

            return workDays;
        }

        public async Task<ActionResult> DetailList()
        {
            GridRequest request = new GridRequest(Request);
            var listDeps = GetCurUserDepartList().Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            string memberName = Request["memberName"];
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var listAttendance = _attendanceContract.Attendances.Where(predicate)
                                 .Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == month);
                if (departmentId != 0)
                {
                    listAttendance = from a in listAttendance
                                     where a.Administrator.DepartmentId == departmentId
                                     select a;
                }
                else
                {
                    listAttendance = from a in listAttendance
                                     where listDeps.Contains(a.Administrator.DepartmentId.Value)
                                     select a;
                }

                if (!string.IsNullOrEmpty(memberName))
                {
                    listAttendance = from a in listAttendance
                                     where a.Administrator.Member.MemberName.Contains(memberName)
                                     select a;
                }

                List<int> listAdminId = listAttendance.Select(x => x.AdminId).Distinct().ToList();
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => listAdminId.Contains(x.Id) && !x.IsDeleted && x.IsEnabled).ToList();
                DateTime atteTime = DateTime.Parse(year + "年" + month + "月");
                int currentDays = DateTime.DaysInMonth(year, month);
                List<AttendanceInfo> listAttendanceInfo = new List<AttendanceInfo>();
                count = listAdmin.Count();
                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                var list = listAdmin.Skip(pageIndex).Take(pageSize).Select(x => x.Id);
                var data_list = new List<object>();
                foreach (int item in list)
                {
                    var att_info = GetAttendanceInfo(item, year, month);
                    if (att_info != null)
                    {
                        data_list.Add(new
                        {
                            Id = item,
                            Date = year + "年" + month + "月",
                            LateCount = att_info.LateCount,
                            LeaveEarlyCount = att_info.LeaveEarlyCount,
                            LeaveDays = att_info.LeaveDays,
                            NormalDays = att_info.NormalDays,
                            FieldDays = att_info.FieldDays,
                            OvertimeDays = att_info.OvertimeDays,
                            RealName = att_info.RealName,
                            NoSignOutCount = att_info.NoSignOutCount
                        });
                    }
                }
                return new GridData<object>(data_list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        /// <summary>
        /// 获取员工考勤信息
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="curr_year"></param>
        /// <param name="curr_month"></param>
        /// <returns></returns>
        public AttendanceInfo GetAttendanceInfo(int adminId, int curr_year, int curr_month)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
            if (admin != null)
            {
                int days = DateTime.DaysInMonth(curr_year, curr_month);
                var curr_startTime = DateTime.Parse(curr_year.ToString() + "-" + curr_month.ToString().PadLeft(2, '0') + "-01 0:00");
                var curr_endTime = DateTime.Parse(curr_year.ToString() + "-" + curr_month.ToString().PadLeft(2, '0') + "-" + days.ToString() + " 23:59");
                AttendanceInfo ad = new AttendanceInfo();

                ad.NormalDays = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId &&
                  x.AttendanceTime.Year == curr_year && x.AttendanceTime.Month == curr_month).Count();
                ad.OvertimeDays = _overtimeContract.Overtimes.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1 &&
                x.StartTime >= curr_startTime && x.EndTime <= curr_endTime).ToList().Sum(x => x.OvertimeDays);
                ad.LeaveDays = _leaveInfoContract.LeaveInfos.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1 &&
    x.StartTime >= curr_startTime && x.EndTime <= curr_endTime && x.LeaveMethod == 1).ToList().Sum(x => x.LeaveDays);
                ad.FieldDays = _fieldContract.Fields.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.VerifyType == 1 &&
    x.StartTime >= curr_startTime && x.EndTime <= curr_endTime).ToList().Sum(x => x.FieldDays);
                ad.LateCount = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId &&
                    x.AttendanceTime.Year == curr_year && x.AttendanceTime.Month == curr_month && x.IsLate == -1).Count();
                ad.LeaveEarlyCount = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId &&
                     x.AttendanceTime.Year == curr_year && x.AttendanceTime.Month == curr_month && x.IsLeaveEarly == -1).Count();
                ad.NoSignOutCount = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId &&
                     x.AttendanceTime.Year == curr_year && x.AttendanceTime.Month == curr_month && x.IsNoSignOut == -1).Count();
                ad.RealName = admin.Member == null ? "" : admin.Member.RealName;
                return ad;
            }
            else
            {
                return null;
            }
        }


        #region 初始化迟到界面
        /// <summary>
        /// 初始化迟到界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Late()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion

        #region 获取迟到数据
        /// <summary>
        /// 获取迟到数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> LateList()
        {
            GridRequest request = new GridRequest(Request);
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            //string LeaveInfoId = Request["LeaveInfoId"];
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var listAttendance = _attendanceContract.Attendances.Where(predicate)
                .Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == month)
                .Where(x => x.Administrator.DepartmentId == departmentId);

                //if (!string.IsNullOrEmpty(LeaveInfoId))
                //{
                //    listAttendance = listAttendance.Where(x => x.LeaveInfoId != null);
                //}

                var dataSouce = from a in listAttendance
                                group a by new { a.AdminId, a.Administrator.Member.RealName } into g
                                select new
                                {
                                    AdminId = g.Key.AdminId,
                                    RealName = g.Key.RealName,
                                    Count = g.Count()
                                };
                count = dataSouce.Count();
                var listEntity = dataSouce.OrderByDescending(x => x.Count).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x => new
                {
                    x.AdminId,
                    x.RealName,
                    x.Count,
                });
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取未签退数据
        /// <summary>
        /// 获取未签退数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> NoSignOutList()
        {
            GridRequest request = new GridRequest(Request);
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            //string LeaveInfoId = Request["LeaveInfoId"];
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var listAttendance = _attendanceContract.Attendances.Where(predicate)
                .Where(x => x.AttendanceTime.Year == year && x.AttendanceTime.Month == month)
                .Where(x => x.Administrator.DepartmentId == departmentId);

                var dataSouce = from a in listAttendance
                                group a by new { a.AdminId, a.Administrator.Member.RealName } into g
                                select new
                                {
                                    AdminId = g.Key.AdminId,
                                    RealName = g.Key.RealName,
                                    Count = g.Count()
                                };
                count = dataSouce.Count();
                var listEntity = dataSouce.OrderByDescending(x => x.Count).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x => new
                {
                    x.AdminId,
                    x.RealName,
                    x.Count,
                });
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取请假数据
        public JsonResult LeaveList()
        {
            GridRequest request = new GridRequest(Request);
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            //string LeaveInfoId = Request["LeaveInfoId"];
            DateTime curr_startTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + " 01 0:00");
            DateTime curr_endTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(year, month) + " 23:59");
            Expression<Func<LeaveInfo, bool>> predicate = FilterHelper.GetExpression<LeaveInfo>(request.FilterGroup);
            var count = 0;
            var listAttendance = _leaveInfoContract.LeaveInfos
            .Where(x => !x.IsDeleted && x.IsEnabled && x.StartTime >= curr_startTime && x.EndTime <= curr_endTime);
            var dep_list = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departmentId);
            var dataSouce = from a in listAttendance
                            join b in dep_list on a.AdminId equals b.Id
                            where a.VerifyType == 1
                            group b by new { b.Id, b.Member.RealName } into g
                            select new
                            {
                                AdminId = g.Key.Id,
                                RealName = g.Key.RealName,
                                Count = g.Count()
                            };
            count = dataSouce.Count();
            var listEntity = dataSouce.OrderByDescending(x => x.Count).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x => new
            {
                x.AdminId,
                x.RealName,
                x.Count,
            });
            return Json(new GridData<object>(listEntity, count, request.RequestInfo), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取外勤数据
        public async Task<ActionResult> FileList()
        {
            GridRequest request = new GridRequest(Request);
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            //string LeaveInfoId = Request["LeaveInfoId"];
            DateTime curr_startTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + " 01 0:00");
            DateTime curr_endTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(year, month) + " 23:59");
            Expression<Func<LeaveInfo, bool>> predicate = FilterHelper.GetExpression<LeaveInfo>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var listAttendance = _fieldContract.Fields
                .Where(x => !x.IsDeleted && x.IsEnabled && x.StartTime >= curr_startTime && x.EndTime <= curr_endTime);
                var dep_list = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departmentId);
                var dataSouce = from a in listAttendance
                                join b in dep_list on a.AdminId equals b.Id
                                where a.VerifyType == 1
                                group b by new { b.Id, b.Member.RealName } into g
                                select new
                                {
                                    AdminId = g.Key.Id,
                                    RealName = g.Key.RealName,
                                    Count = g.Count()
                                };
                count = dataSouce.Count();
                var listEntity = dataSouce.OrderByDescending(x => x.Count).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x => new
                {
                    x.AdminId,
                    x.RealName,
                    x.Count,
                });
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取加班数据
        public JsonResult OverTimeList()
        {
            GridRequest request = new GridRequest(Request);
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            //string LeaveInfoId = Request["LeaveInfoId"];
            DateTime curr_startTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + " 01 0:00");
            DateTime curr_endTime = DateTime.Parse(year.ToString() + "-" + month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(year, month) + " 23:59");
            Expression<Func<LeaveInfo, bool>> predicate = FilterHelper.GetExpression<LeaveInfo>(request.FilterGroup);
            var count = 0;
            var listAttendance = _overtimeContract.Overtimes
            .Where(x => !x.IsDeleted && x.IsEnabled && x.StartTime >= curr_startTime && x.EndTime <= curr_endTime);
            var dep_list = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == departmentId);
            var dataSouce = from a in listAttendance
                            join b in dep_list on a.AdminId equals b.Id
                            where a.VerifyType == 1
                            group b by new { b.Id, b.Member.RealName } into g
                            select new
                            {
                                AdminId = g.Key.Id,
                                RealName = g.Key.RealName,
                                Count = g.Count()
                            };
            count = dataSouce.Count();
            var listEntity = dataSouce.OrderByDescending(x => x.Count).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).Select(x => new
            {
                x.AdminId,
                x.RealName,
                x.Count,
            });
            return Json(new GridData<object>(listEntity, count, request.RequestInfo), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 初始化早退界面
        /// <summary>
        /// 初始化早退界面
        /// </summary>
        /// <returns></returns>
        public ActionResult LeaveEarly()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion

        #region 初始化未签退界面
        /// <summary>
        /// 初始化未签退界面
        /// </summary>
        /// <returns></returns>
        public ActionResult NoSignOut()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion



        #region 初始化请假界面
        /// <summary>
        /// 初始化请假界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Leave()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion
        #region 初始化外勤界面
        /// <summary>
        /// 初始化外勤界面
        /// </summary>
        /// <returns></returns>
        public ActionResult File()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion
        #region 初始化加班界面
        /// <summary>
        /// 初始化加班界面
        /// </summary>
        /// <returns></returns>
        public ActionResult OverTime()
        {
            int year = string.IsNullOrEmpty(Request["year"]) ? 0 : Convert.ToInt32(Request["year"]);
            int month = string.IsNullOrEmpty(Request["month"]) ? 0 : Convert.ToInt32(Request["month"]);
            int departmentId = string.IsNullOrEmpty(Request["DepartmentId"]) ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.DepartmentId = departmentId;
            return PartialView();
        }
        #endregion

        #region 初始化正常上班界面
        /// <summary>
        /// 初始化正常上班界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Normal()
        {
            string strDate = Request["Date"];
            string[] arrDate = GetDate(strDate);
            ViewBag.StartTime = arrDate[0];
            ViewBag.EndTime = arrDate[1];
            return PartialView();
        }
        #endregion

        #region 计算出天数
        private string[] GetDate(string strDate)
        {
            DateTime startDate = DateTime.Parse(strDate);
            int day = DateTime.DaysInMonth(startDate.Year, startDate.Month) - 1;
            DateTime endDate = startDate.AddDays(day);
            string[] arr = { startDate.ToString("yyyy/MM/dd 00:00:00"), endDate.ToString("yyyy/MM/dd 23:59:59") };
            return arr;
        }
        #endregion
    }

    public class AttendanceSeach
    {
        public string Id { get; set; }

        public string Date { get; set; }

        public double LateCount { get; set; }

        public double LeaveEarlyCount { get; set; }

        public double NoSignOutCount { get; set; }

        public double LeaveDays { get; set; }

        public double AbsenceDays { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public string ParentId { get; set; }

        public double OverTimeDays { get; set; }
        public int orderId { get; set; }
    }
}