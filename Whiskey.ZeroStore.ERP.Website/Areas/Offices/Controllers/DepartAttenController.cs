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
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Xml;
using System.Xml.Linq;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class DepartAttenController : Controller
    {

        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(DepartAttenController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly ILeaveInfoContract _leaveInfoContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IOvertimeContract _overtimeContract;

        protected readonly IHolidayContract _holidayContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IStoreContract _storeContract;
        protected readonly IAttendanceRepairContract _attendanceRepairContract;
        protected readonly IConfigureContract _configureContract;
        public DepartAttenController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            ILeaveInfoContract leaveInfoContract,
            IWorkTimeContract workTimeContract,
            IOvertimeContract overtimeContract,
            IHolidayContract holidayContract,
            IStoreContract storeContract,
            IMemberContract memberContract,
            IAttendanceRepairContract attendanceRepairContract,
            IConfigureContract configureContract)
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
            _memberContract = memberContract;
            _attendanceRepairContract = attendanceRepairContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 初始化数据
        [Layout]
        public ActionResult Index()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            bool isPower = false;
            int count = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.SubordinateId == adminId).Count();
            int index = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.JobPosition.IsLeader == true && x.Id == adminId).Count();
            Administrator admin = _administratorContract.Administrators.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == adminId);
            int tempCount = 0;
            if (admin.JobPositionId != null)
            {
                List<Department> listDepart = admin.JobPosition.Departments.ToList();
                if (listDepart != null)
                {
                    tempCount = listDepart.Count();
                }
            }
            if (count > 0 || index > 0 || tempCount > 0)
            {
                isPower = true;
            }

            List<SelectListItem> listDp = new List<SelectListItem>();
            listDp.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            listDp.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.Department = listDp;
            ViewBag.Power = isPower;
            ViewBag.AdminId = adminId;
            return View();
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取考勤数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            int adminId = AuthorityHelper.OperatorId ?? 0;
            int departId = 0;
            int attenType = 0;
            FilterRule ruleType = request.FilterGroup.Rules.Where(x => x.Field == "AttenType").FirstOrDefault();
            if (ruleType != null)
            {
                attenType = int.Parse(ruleType.Value.ToString());
                request.FilterGroup.Rules.Remove(ruleType);
            }
            FilterRule rule = request.FilterGroup.Rules.Where(x => x.Field == "DepartId").FirstOrDefault();
            if (rule != null)
            {
                departId = int.Parse(rule.Value.ToString());
                request.FilterGroup.Rules.Remove(rule);
            }
            FilterRule filterRule = request.FilterGroup.Rules.Where(x => x.Field == "RealName").FirstOrDefault();
            string strRealName = string.Empty;
            if (filterRule != null)
            {
                strRealName = filterRule.Value.ToString();
                request.FilterGroup.Rules.Remove(filterRule);
            }
            Expression<Func<Attendance, bool>> predicate = FilterHelper.GetExpression<Attendance>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                Administrator admin = listAdmin.FirstOrDefault(x => x.Id == adminId);
                bool res = admin.JobPosition != null && admin.JobPosition.IsLeader;
                List<int> listAdminId = new List<int>();
                if (!string.IsNullOrEmpty(strRealName))
                {
                    listAdmin = listAdmin.Where(x => x.Member.RealName.Contains(strRealName)).ToList();
                }
                List<Department> listDepart = new List<Department>();
                List<int> departmentIdList = new List<int>();
                var currDep = Utils.GetCurUserDepartList(adminId, _administratorContract, false);

                foreach (var item in currDep)
                {
                    departmentIdList.Add(Convert.ToInt32(item.Value));
                }
                var ab = (from de in _departmentContract.Departments
                          join c in departmentIdList
                          on de.Id equals c
                          select de).ToList();
                if (ab.Count > 0)
                {
                    listDepart.AddRange(ab);
                }
                if (adminId == admin.Department.SubordinateId)
                {
                    List<Department> children = admin.Department.Children.ToList();
                    foreach (var item in children)
                    {
                        if (!listDepart.Exists(x => x.Id == item.Id))
                        {
                            listDepart.Add(item);
                        }
                    }
                }
                if (listDepart != null)
                {
                    List<int> childrenId = listDepart.Select(x => x.Id).ToList();
                    if (departId != 0)
                    {
                        childrenId = listDepart.Where(x => x.Id == departId).Select(x => x.Id).ToList();
                    }
                    List<Administrator> listEntity = listAdmin.Where(x => childrenId.Contains(x.DepartmentId ?? 0)).ToList();
                    if (listEntity != null && listEntity.Count() > 0)
                    {
                        listAdminId.AddRange(listEntity.Select(x => x.Id).ToList());
                    }
                }
                IQueryable<Attendance> listAttendance = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true).OrderByDescending(x => x.CreatedTime);
                listAttendance = listAttendance.Where(x => listAdminId.Contains(x.AdminId));
                if (attenType != 0)
                {
                    listAttendance = FilterData(listAttendance, attenType);
                }

                var list = listAttendance.Where<Attendance, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Administrator.Department.DepartmentName,
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
                    l_StartTime = x.LeaveInfo == null ? "" : x.LeaveInfo.StartTime.ToString(),
                    l_EndTime = x.LeaveInfo == null ? "" : x.LeaveInfo.EndTime.ToString(),
                    x.AttendanceTime,
                    x.LateMinutes,
                    x.LeaveEarlyMinutes,
                    x.OvertimeId,
                    O_StartTime = x.Overtime == null ? "" : x.Overtime.StartTime.ToString(),
                    O_EndTime = x.Overtime == null ? "" : x.Overtime.EndTime.ToString(),
                    x.FieldId,
                    x.AbsenceType,
                    x.OvertimeType,
                    x.FieldType,
                    x.LeaveInfoType,
                    x.IsNoSignOut
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(AttendanceDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                string strAdminId = Request["AdminId"];
                if (string.IsNullOrEmpty(strAdminId.Trim()))
                {
                    oper.Message = "请选择员工";
                    return Json(oper);
                }
                List<int> adminIds = strAdminId.Split(",").Where(a => !string.IsNullOrEmpty(a)).Select(a => int.Parse(a)).ToList();

                var attends = _attendanceContract.Attendances.Where(x => !x.IsDeleted && x.IsEnabled && adminIds.Contains(x.AdminId) && x.AttendanceTime.ToShortDateString().Equals(dto.AttendanceTime.ToShortDateString()));
                int count = 0;
                if (attends != null)
                {
                    var adm_rmv = attends.Select(x => x.AdminId);
                    adminIds.RemoveAll(a => adm_rmv.Contains(a));
                    count = adm_rmv.Count();
                }
                if (adminIds == null
                    && adminIds.Count() == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "您选择的员工均有当天签到记录"));
                }
                oper = _attendanceContract.CreateOrUpdate(dto, strAdminId);
                if (count > 0)
                {
                    oper.Message += ",有" + count + "个员工有当天签到记录";
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                oper.Message = "服务器忙，请稍后访问";
                _Logger.Error<string>(ex.ToString());
                return Json(oper);
            }
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            AttendanceDto entity = _attendanceContract.Edit(Id);
            Administrator admin = _administratorContract.View(entity.AdminId);
            ViewBag.RealName = admin.Member.RealName;
            return PartialView(entity);
        }

        /// <summary>
        /// 保存修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(AttendanceDto dto, ApiAttenFlag attenFlag)
        {
            AttendanceDto atten = _attendanceContract.Edit(dto.Id);
            if (atten == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "数据不存在"));
            }
            int flag = (int)attenFlag;
            OperationResult oper = _attendanceRepairContract.ApplyRepair(atten.AdminId, dto.Id, flag);
            AttendanceRepair attendancerepair = _attendanceRepairContract.AttendanceRepairs.FirstOrDefault(x => x.AttendanceId == dto.Id && x.ApiAttenFlag == flag);
            if (attendancerepair == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "数据不存在"));
            }
            AttendanceRepairDto apair = _attendanceRepairContract.Edit(attendancerepair.Id);
            apair.VerifyType = (int)VerifyFlag.Pass;
            oper = _attendanceRepairContract.Verify(apair);
            return Json(oper);
        }
        #endregion

        #region 员工
        public ActionResult Admin()
        {

            ViewBag.AdminId = AuthorityHelper.OperatorId ?? 0;
            return PartialView();
        }
        #endregion

        #region 获取员工数据列表
        /// <summary>
        /// 获取考勤数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            var rule = request.FilterGroup.Rules.Where(x => x.Field == "AdminId").FirstOrDefault();
            string strAdminId = rule.Value.ToString();
            request.FilterGroup.Rules.Remove(rule);
            int adminId = int.Parse(strAdminId);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                Administrator admin = listAdmin.FirstOrDefault(x => x.Id == adminId);
                //bool res = admin.JobPosition == null && admin.JobPosition.IsLeader;
                List<Administrator> listEntity = new List<Administrator>();
                List<int> listDepartId = new List<int>();
                if (admin.JobPosition == null)
                {
                    if (admin.JobPosition.IsLeader == true)
                    {
                        listEntity = listAdmin.Where(x => x.DepartmentId == admin.DepartmentId).ToList();
                    }
                    ICollection<Department> listDepart = admin.JobPosition.Departments;
                    if (listDepart != null && listDepart.Count() > 0)
                    {
                        listDepartId = listDepart.Select(x => x.Id).ToList();
                        listEntity.AddRange(listAdmin.Where(x => listDepartId.Contains(x.DepartmentId ?? 0)));
                    }
                }
                if (adminId == admin.Department.SubordinateId)
                {
                    List<Department> listDepart = admin.Department.Children.ToList();
                    if (listDepart != null)
                    {
                        List<int> childrenId = listDepart.Select(x => x.Id).ToList();
                        listEntity.AddRange(listAdmin.Where(x => childrenId.Contains(x.DepartmentId ?? 0)).ToList());
                    }
                }
                var list = listEntity.Distinct().AsQueryable().Where<Administrator, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Member.RealName,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印和导出数据
        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _attendanceContract.Attendances.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public FileResult Export(int DepartmentId, DateTime StartDate, DateTime EndDate, int AdminId = 0)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            //var list = _attendanceContract.Attendances.ToList();
            var list = GetAttens(DepartmentId, StartDate, EndDate, AdminId);
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            string str1 = st.ToString();
            byte[] fileContents = Encoding.UTF8.GetBytes(str1);
            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "考勤记录.xls");
        }
        #endregion

        #region 初始化导出界面

        public ActionResult ExportIndex()
        {
            List<SelectListItem> listDp = new List<SelectListItem>();

            listDp.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            listDp.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.Department = listDp;
            return PartialView();
        }
        #endregion

        #region 获取要导出的考勤数据
        private List<M_DepartAtten> GetAttens(int departmentId, DateTime startDate, DateTime endDate, int currentAdminId = 0)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            List<M_DepartAtten> listEntity = new List<M_DepartAtten>();
            int count = startDate.CompareTo(endDate);
            int days = 0;
            if (count > 0)
            {
                return listEntity;
            }
            else
            {
                TimeSpan timeSpan = endDate - startDate;
                days = timeSpan.Days + 1;
                List<int> listAdminId = new List<int>();
                List<Administrator> admins = new List<Administrator>();
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                if (currentAdminId == 0)
                {
                    #region 获取部门下的员工
                    List<Administrator> entities = listAdmin.Where(x => x.DepartmentId == departmentId).ToList();
                    if (entities != null && entities.Count() > 0)
                    {
                        listAdminId = listAdmin.Select(x => x.Id).ToList();
                        admins.AddRange(entities);
                    }

                    if (admins.Count == 0)
                    {
                        return listEntity;
                    }
                    #endregion
                }
                else
                {
                    listAdminId.Add(currentAdminId);
                    Administrator admin = listAdmin.FirstOrDefault(x => x.Id == currentAdminId);
                    admins.Add(admin);
                }
                int index = 0;
                Dictionary<string, int> dic = _holidayContract.GetHoliday();
                List<Attendance> listAtten = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && listAdminId.Contains(x.AdminId) && startDate.CompareTo(x.AttendanceTime) <= 0 && endDate.CompareTo(x.AttendanceTime) >= 0).ToList();
                StringBuilder sbContent = new StringBuilder();
                while (true)
                {
                    if (index == days)
                    {
                        break;
                    }
                    else
                    {
                        DateTime currentDate = startDate.AddDays(index);
                        foreach (Administrator admin in admins)
                        {
                            M_DepartAtten mEntity = new M_DepartAtten()
                            {
                                AttenTime = currentDate,
                                RealName = admin.Member.RealName,
                            };
                            WorkTime workTime = new WorkTime();
                            if (admin.WorkTime != null && admin.WorkTime.IsEnabled == true)
                            {
                                workTime = admin.WorkTime;

                            }
                            else if (admin.JobPosition != null)
                            {
                                workTime = admin.JobPosition.WorkTime;
                            }
                            Attendance atten = listAtten.FirstOrDefault(x => x.AdminId == admin.Id && x.AttendanceTime.Year == currentDate.Year && x.AttendanceTime.Month == currentDate.Month && x.AttendanceTime.Day == currentDate.Day);
                            if (atten == null)
                            {
                                bool isWork = OfficeHelper.IsWorkDay(currentDate, workTime, dic);
                                if (isWork == true)
                                {
                                    mEntity.AttenType = "缺勤";
                                }
                                else
                                {
                                    bool isOverTime = IsOverTime(currentDate, admin.Id);
                                    if (isOverTime == true)
                                    {
                                        mEntity.AttenType = "未加班（申请加班）";
                                    }
                                }
                            }
                            else
                            {
                                #region 逻辑判断
                                if (atten.IsLate == -1)
                                {
                                    sbContent.Append("迟到,");
                                }
                                else if (atten.IsLate == 1)
                                {
                                    sbContent.Append("迟到补卡,");
                                }
                                if (atten.IsLeaveEarly == -1)
                                {
                                    sbContent.Append("早退,");
                                }
                                else if (atten.IsLeaveEarly == 1)
                                {
                                    sbContent.Append("早退补卡,");
                                }
                                if (atten.IsNoSignOut == -1)
                                {
                                    sbContent.Append("未签到");
                                }
                                else if (atten.IsNoSignOut == 1)
                                {
                                    sbContent.Append("未签到补卡,");
                                }
                                if (atten.IsAbsence == -1)
                                {
                                    if (atten.AbsenceType == (int)AttendanceFlag.AmAbsence)
                                    {
                                        sbContent.Append("上午缺勤,");
                                    }
                                    else if (atten.AbsenceType == (int)AttendanceFlag.PmAbsence)
                                    {
                                        sbContent.Append("下午缺勤,");
                                    }
                                    else
                                    {
                                        sbContent.Append("缺勤,");
                                    }
                                }
                                else if (atten.IsAbsence == 1)
                                {
                                    sbContent.Append("缺勤补卡,");
                                }
                                if (atten.FieldId != null)
                                {
                                    if (atten.FieldType == (int)AttendanceFlag.AmField)
                                    {
                                        sbContent.Append("上午外勤,");
                                    }
                                    else if (atten.FieldType == (int)AttendanceFlag.PmField)
                                    {
                                        sbContent.Append("下午外勤,");
                                    }
                                    else
                                    {
                                        sbContent.Append("外勤,");
                                    }
                                }
                                if (atten.LeaveInfoId != null)
                                {
                                    if (atten.FieldType == (int)AttendanceFlag.AmLeave)
                                    {
                                        sbContent.Append("上午请教,");
                                    }
                                    else if (atten.FieldType == (int)AttendanceFlag.PmLeave)
                                    {
                                        sbContent.Append("下午请假,");
                                    }
                                    else
                                    {
                                        sbContent.Append("请假,");
                                    }
                                }
                                #endregion
                                mEntity.AttenType = sbContent.ToString();
                                mEntity.StartTime = atten.AmStartTime;
                                mEntity.EndTime = atten.PmEndTime;
                            }
                            listEntity.Add(mEntity);
                            sbContent.Clear();
                        }
                        index++;
                    }
                }
            }
            return listEntity;
        }
        #endregion

        #region 校验是否为工作日
        //private bool IsWorkDay(DateTime currentDate,Administrator admin)
        //{

        //}
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

        #region 获取日期
        private DateTime GetDate(string strDate)
        {
            string strTemp = string.Empty;
            int year = GetNum(strDate, "/", out strTemp);
            int month = GetNum(strTemp, "月", out strTemp);
            int day = GetNum(strTemp, "日", out strTemp);
            DateTime dateTime = new DateTime(year, month, day);
            return dateTime;
        }

        private int GetNum(string strDate, string strWord, out string strTemp)
        {
            string[] arrStr = strDate.Split(strWord);
            string strNum = arrStr[0].ToString().Trim();

            //strNum = "2016";
            int t = Convert.ToInt32(strNum);
            int num = int.Parse(strNum);
            strTemp = arrStr[1];
            return num;
        }
        #endregion

        #region 根据部门获取员工
        [HttpPost]
        public JsonResult GetAdmin(int DepartId)
        {
            List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == DepartId).ToList();
            var list = listAdmin.Select(x => new
            {
                x.Id,
                x.Member.RealName,
            }).ToList();
            return Json(list);
        }
        #endregion

        #region 获取管理的部门
        private List<SelectListItem> GetDepartments(int adminId)
        {
            Administrator admin = _administratorContract.View(adminId);
            List<int> listId = new List<int>();
            List<Department> listDepart = new List<Department>();
            if (admin.JobPosition != null && admin.JobPosition.IsLeader == true)
            {
                listId.Add(admin.DepartmentId ?? 0);
                listDepart.Add(admin.Department);
            }
            List<Department> listDepartment = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            Department depart = listDepartment.FirstOrDefault(x => x.SubordinateId == adminId);
            if (depart != null)
            {
                listDepart.AddRange(depart.Children.ToList());
            }
            List<SelectListItem> select = listDepart.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.DepartmentName,
            }).ToList();
            return select;
        }
        #endregion

        #region 通过考勤状态筛选数据
        private IQueryable<Attendance> FilterData(IQueryable<Attendance> listAttendance, int attenType)
        {
            List<Attendance> list = new List<Attendance>();
            switch (attenType)
            {
                case (int)ApiAttenFlag.Late:
                    listAttendance = listAttendance.Where(x => x.IsLate == -1);
                    break;
                case (int)ApiAttenFlag.LeaveEarly:
                    listAttendance = listAttendance.Where(x => x.IsLeaveEarly == -1);
                    break;
                case (int)ApiAttenFlag.NoSignOut:
                    listAttendance = listAttendance.Where(x => x.IsNoSignOut == -1);
                    break;
                case 100:
                    listAttendance = listAttendance.Where(x => (x.IsLate == 1) || (x.IsLeaveEarly == 1) || (x.IsNoSignOut == 1));
                    break;
                default:
                    break;
            }
            return listAttendance;
        }
        #endregion

        public ActionResult LeavePointsIndex()
        {
            //string LeavePoints = Utility.XmlStaticHelper.GetXmlNodeByXpath("LeavePoints", "LeavePointsconfig", "LeavePoints");
            string LeavePoints = _configureContract.GetConfigureValue("LeavePoints", "LeavePointsconfig", "LeavePoints");
            ViewBag.LeavePoints = LeavePoints;
            return PartialView();
        }

        public JsonResult LeavePoints(string leavePoints)
        {
            //var status = Utility.XmlStaticHelper.UpdateNode("LeavePoints", "LeavePointsconfig", "LeavePoints", leavePoints);
            var status = _configureContract.SetConfigure("LeavePoints", "LeavePointsconfig", "LeavePoints", leavePoints);
            //return Json("1");
            return Json(OperationHelper.ReturnOperationResult(status, "考勤积分配置"));
        }
        public ActionResult PunchOutIndex()
        {
            //string PunchOut = Utility.XmlStaticHelper.GetXmlNodeByXpath("PunchOut", "PunchOutconfig", "PunchOut");
            string PunchOut = _configureContract.GetConfigureValue("PunchOut", "PunchOutconfig", "PunchOut");
            ViewBag.PunchOut = PunchOut;
            return PartialView();
        }

        public JsonResult PunchOut(string PunchOut)
        {
            //var status = Utility.XmlStaticHelper.UpdateNode("PunchOut", "PunchOutconfig", "PunchOut", PunchOut);
            var status = _configureContract.SetConfigure("PunchOut", "PunchOutconfig", "PunchOut", PunchOut);
            //return Json("1");
            return Json(OperationHelper.ReturnOperationResult(status, "打卡签退配置"));
        }
    }
}