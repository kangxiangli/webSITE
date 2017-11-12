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
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    /// <summary>
    /// 员工考勤
    /// </summary>
    [License(CheckMode.Verify)]
    public class StaffAttendanceController : BaseController
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

        protected readonly IHolidayContract _holidayContract;

        protected readonly IStoreContract _storeContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IAttendanceRepairContract _attendanceRepairContract;
        protected readonly IWorkOrderDealtWithContract _workOrderDealtWithContract;
        protected readonly IExamRecordContract _examRecordContract;
        protected readonly IStoreStatisticsContract _statContract;

        public StaffAttendanceController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            ILeaveInfoContract leaveInfoContract,
            IWorkTimeContract workTimeContract,
            IOvertimeContract overtimeContract,
            IHolidayContract holidayContract,
            IStoreContract storeContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IAttendanceRepairContract attendanceRepairContract,
            IWorkOrderDealtWithContract workOrderDealtWithContract,
            IExamRecordContract examRecordContract,
            IStoreStatisticsContract statContract
            )
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _leaveInfoContract = leaveInfoContract;
            _workTimeContract = workTimeContract;
            _overtimeContract = overtimeContract;
            _holidayContract = holidayContract;
            _storeContract = storeContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _attendanceRepairContract = attendanceRepairContract;
            _workOrderDealtWithContract = workOrderDealtWithContract;
            _examRecordContract = examRecordContract;
            _statContract = statContract;
        }
        #endregion

        #region 初始化考勤界面
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

        #region 获取考勤数据列表
        /// <summary>
        /// 获取考勤数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string strAdminId = request.FilterGroup.Rules.Where(x => x.Field == "AdminId").FirstOrDefault().Value.ToString();
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                int adminId = int.Parse(strAdminId);
                var listEntity = _attendanceContract.Attendances.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
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
                    x.OvertimeId,
                    x.FieldId,
                    x.AbsenceType,
                    x.OvertimeType,
                    x.FieldType,
                    x.LeaveInfoType,
                    x.IsNoSignOut
                }).ToList();
                return new GridData<object>(listEntity, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 是否补卡
        /// </summary>
        /// <param name="attendanceId"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool GetIsAttendanceRepair(int attendanceId, ApiAttenFlag flag)
        {
            int verifyFlag = Convert.ToInt32(VerifyFlag.Pass);
            int apiattenflag = Convert.ToInt32(flag);
            var attendance = _attendanceRepairContract.AttendanceRepairs.FirstOrDefault(a => a.AttendanceId == attendanceId && a.VerifyType == verifyFlag && a.ApiAttenFlag == apiattenflag);
            return attendance == null ? false : true;
        }
        #endregion

        #region 打卡

        #region 初始化打卡界面

        /// <summary>
        /// 初始化打卡界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Registration()
        {
            Dictionary<string, int> dic = _holidayContract.GetHoliday();
            string strStartTime = "";
            string strEndTime = "";
            string strLoginInTime = string.Empty;
            string strLoginOutTime = string.Empty;
            int adminId = AuthorityHelper.OperatorId ?? 0;
            //初始值默认不能签到和签退的
            bool enableLoginIn = false;
            bool enableLoginOut = false;
            bool Isatten = false;
            var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            if (dic == null && dic.Count == 0)
            {
                strStartTime = "请添加公休假";
                strEndTime = "请添加公休假";
            }
            else
            {
                OperationResult oper = _administratorContract.GetWorkTime(adminId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    WorkTime workTime = oper.Data as WorkTime;
                    DateTime nowTime = DateTime.Now;
                    Attendance atten = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId)
                        .FirstOrDefault(x => nowTime.Year == x.AttendanceTime.Year && nowTime.Month == x.AttendanceTime.Month && nowTime.Day == x.AttendanceTime.Day);

                    bool isWork = IsWorkDay(nowTime, workTime, dic);
                    string strNotice = "弹性时间";
                    //提示上下班时间
                    if (!admin.IsPersonalTime)
                    {
                        strStartTime = workTime.IsFlexibleWork == true ? strNotice : workTime.AmStartTime;
                        strEndTime = workTime.IsFlexibleWork == true ? strNotice : workTime.PmEndTime;
                    }
                    else
                    {
                        var _wd = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId &&
                        x.Year == nowTime.Year && x.Month == nowTime.Month && x.WorkDay == nowTime.Day);
                        if (_wd != null)
                        {
                            if (_wd.WorkTimeType != 2)
                            {
                                strStartTime = workTime.IsFlexibleWork == true ? strNotice : _wd.AmStartTime;
                                strEndTime = workTime.IsFlexibleWork == true ? strNotice : _wd.PmEndTime;
                            }
                            else
                            {
                                isWork = false;
                            }
                        }
                    }

                    if (isWork)
                    {
                        enableLoginIn = true;
                        enableLoginOut = true;
                    }
                    else
                    {
                        isWork = IsOverTime(nowTime, adminId);
                        if (isWork == true)
                        {
                            enableLoginIn = true;
                            enableLoginOut = true;
                        }
                    }
                    if (atten != null)
                    {
                        enableLoginIn = false;
                        Isatten = true;
                        strLoginInTime = atten.AmStartTime;
                        strLoginOutTime = atten.PmEndTime;
                        if (atten.LeaveInfoId != null)
                        {
                            enableLoginIn = true;
                            enableLoginOut = true;
                            enableLoginIn = true;
                        }
                        if (atten.FieldId != null)
                        {
                            enableLoginIn = true;
                            enableLoginOut = true;
                            enableLoginIn = true;
                        }
                    }
                }
            }
            ViewBag.EnableLoginIn = enableLoginIn;
            ViewBag.EnableLoginOut = enableLoginOut;
            ViewBag.LoginInTime = strLoginInTime;
            ViewBag.LoginOutTime = strLoginOutTime;
            ViewBag.StartTime = strStartTime;
            ViewBag.EndTime = strEndTime;
            return PartialView();
        }

        #endregion

        public string CheckIsWorkDay()
        {
            Dictionary<string, int> dic = _holidayContract.GetHoliday();
            int adminId = AuthorityHelper.OperatorId ?? 0;
            DateTime nowTime = DateTime.Now;
            var admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
            var workTime = new WorkTime();
            if (admin.IsPersonalTime)
            {
                if (admin.WorkTime == null)
                {
                    return "0";
                }
                workTime = admin.WorkTime;
                var _wd = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Year == DateTime.Now.Year &&
x.Month == DateTime.Now.Month && x.WorkDay == DateTime.Now.Day);
                workTime.AmStartTime = _wd.AmStartTime;
                workTime.PmEndTime = _wd.PmEndTime;
            }
            else
            {
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    return "0";
                }
                workTime = admin.JobPosition.WorkTime;

            }
            bool isWork = IsWorkDay(nowTime, workTime, dic);
            if (IsOverTime(nowTime, adminId))
            {
                isWork = true;
            }
            return isWork ? "1" : "0";
        }


        #region 校验工作时间
        /// <summary>
        /// 校验是否为工作时间
        /// </summary>
        /// <param name="attenTime"></param>
        /// <param name="workTime"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public bool IsWorkDay(DateTime attenTime, WorkTime workTime, Dictionary<string, int> dic)
        {
            string strCurrentWeek = attenTime.DayOfWeek.ToString("d");
            string strCurrentDate = attenTime.ToString("yyyyMMdd");
            string[] arrWeek = workTime.WorkWeek.Split(',');
            int count = 0;
            if (workTime.IsPersonalTime)
            {
                var _wd = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTime.Id && x.Year == attenTime.Year &&
                x.Month == attenTime.Month && x.WorkTimeType != 2).Select(x => x.WorkDay).ToList();
                strCurrentWeek = attenTime.Day.ToString().PadLeft(2, '0');
                arrWeek = _wd.ConvertAll(x => x.ToString().PadLeft(2, '0')).ToArray();
            }
            if (arrWeek.Contains(strCurrentWeek))
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.Holidays).Count();
                    }
                    if (count > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.WorkDay).Count();
                    }
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion
        #region 校验是否加班
        private bool IsOverTime(DateTime attenTime, int adminId)
        {
            bool isOverTime = false;
            int count = _overtimeContract.Overtimes.Where(x => x.AdminId == adminId && x.VerifyType == (int)VerifyFlag.Pass && x.StartTime.CompareTo(attenTime) <= 0 && x.EndTime.CompareTo(attenTime) >= 0).Count();
            if (count > 0)
            {
                isOverTime = true;
            }
            return isOverTime;
        }
        #endregion

        #region 签到
        /// <summary>
        /// 签到
        /// </summary>
        /// <returns></returns>
        public JsonResult LoginIn()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请重新登陆");
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _administratorContract.View(adminId);


            var examRecords = _examRecordContract.Entities.Count(r => r.AdminId == adminId && !r.IsDeleted && r.IsEnabled && r.EntryTrainStatus == 0);
            if (examRecords > 0)
            {
                oper.Message = "您的入职培训试题未完成，无法签到";
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

            if (_workOrderDealtWithContract.Entities.Count(d => d.HandlerID == adminId && d.Status == 0) > 0)
            {
                oper.Message = "您有未处理的配单信息";
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

            var dic = _attendanceContract.DoubleScoreReminderBySign(adminId);

            if (dic["DeductionDoubleScore"])
            {
                oper.Message = "上个月有未处理的补卡，处理后方可正常签到";
                oper.Other = -2;
                return Json(oper, JsonRequestBehavior.AllowGet);
            }

            string strMac = Request["Mac"];

            var checkmac = admin?.JobPosition.CheckMac != false;
            if (checkmac)
            {
                oper = this.CheckMac(strMac);

                if (oper.ResultType != OperationResultType.Success) return Json(oper, JsonRequestBehavior.AllowGet);
            }
            Department department = oper.Data as Department;
            oper = _attendanceContract.LoginIn(adminId, department);

            if (dic["IsReminder"])
            {
                oper.Other = -1;
                oper.Message = "补卡未处理的从下个月进行补卡会扣双倍积分";
            }
            else
            {
                oper.Other = 0;
            }
            admin.UpdatedTime = DateTime.Now;
            admin.LoginTime = DateTime.Now;
            _administratorContract.Update(admin);

            var res = _statContract.SetStoreOpenWhenFirstSignIn(adminId);

            if (oper.ResultType == OperationResultType.Success)
            {
                _administratorContract.SendAdminBirthdayNoti(adminId);
            }

            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 签退
        /// <summary>
        /// 签退
        /// </summary>
        /// <returns></returns>
        public JsonResult LoginOut()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "用户未登录，无法签退");
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string strMac = Request["Mac"];
            Administrator admin = _administratorContract.View(adminId);

            var checkmac = admin?.JobPosition.CheckMac != false;
            if (checkmac)
            {
                oper = this.CheckMac(strMac);

                if (oper.ResultType != OperationResultType.Success) return Json(oper, JsonRequestBehavior.AllowGet);
            }
            Department department = oper.Data as Department;
            oper = _attendanceContract.LoginOut(adminId, department);
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion


        #region 查看考勤详情
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity = _attendanceContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 校验MAC地址
        /// <summary>
        /// 校验MAC地址
        /// </summary>
        /// <param name="strMac"></param>
        /// <returns></returns>
        private OperationResult CheckMac(string strMac)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "暂时无法打卡");
            List<Department> listDepartment = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && !string.IsNullOrEmpty(x.MacAddress)).ToList();
            if (listDepartment.Count() == 0 || string.IsNullOrEmpty(strMac))
            {
                return oper;
            }
            else
            {
                List<string> listMac = listDepartment.Select(x => x.MacAddress).ToList();
                foreach (Department department in listDepartment)
                {
                    if (!string.IsNullOrEmpty(department.MacAddress))
                    {
                        string[] arrMac = department.MacAddress.Split(',');
                        int count = arrMac.Where(x => x == strMac).Count();
                        if (count != 0)
                        {
                            return new OperationResult(OperationResultType.Success, string.Empty, department);
                        }
                    }
                }
                return oper;
            }
        }
        #endregion
    }
}