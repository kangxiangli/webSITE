
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using Antlr3.ST;
using Antlr3.ST.Language;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{

    [License(CheckMode.Verify)]
    public class AdministratorController : BaseController
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AdministratorController));

        protected readonly IAdministratorContract _administratorContract;
        protected readonly IDepartmentContract _departmentContract;
        //protected readonly IAdministratorTypeContract AdministratorTypeContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IMemberContract _memberContract;
        protected readonly ICollocationContract _collocationContract;
        protected readonly IRoleContract _roleContract;
        //protected readonly IGroupContract _groupContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IJobPositionContract _jobPositionContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IWorkTimeContract _workTimeContract;


        public AdministratorController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IStoreContract storeContract,
            IStorageContract storageContract,
            IMemberContract memberContract,
            ICollocationContract collocationContract,
            IRoleContract roleContract,
            //IGroupContract groupContract,
            IPermissionContract permissionContract,
            IModuleContract moduleContract,
            IJobPositionContract jobPositionContract,
            //IAdministratorTypeContract AdministratorTypeContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IWorkTimeContract workTimeContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _memberContract = memberContract;
            _collocationContract = collocationContract;
            _roleContract = roleContract;
            //_groupContract = groupContract;
            _permissionContract = permissionContract;
            _moduleContract = moduleContract;
            _jobPositionContract = jobPositionContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _workTimeContract = workTimeContract;
            //this.AdministratorTypeContract = AdministratorTypeContract;
        }
        #endregion

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            var li = CacheAccess.GetDepartmentListItem(_departmentContract, true, DepartmentTypeFlag.公司, DepartmentTypeFlag.店铺);

            ViewBag.Departments = li;
            //ViewBag.AdministratorTypes = CacheAccess.GetAdministratorTypeListItem(AdministratorTypeContract, true);

            return View();
        }


        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var li = CacheAccess.GetDepartmentListItem(_departmentContract, false, DepartmentTypeFlag.公司, DepartmentTypeFlag.店铺);

            ViewBag.Departments = li;

            //ViewBag.AdministratorTypes = CacheAccess.GetAdministratorTypeListItem(AdministratorTypeContract, false).Take(1).ToList();

            Dictionary<int, string> dic = new Dictionary<int, string>();

            var rols = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in rols)
            {
                dic.Add(item.Id, item.RoleName);
            }
            ViewBag.roles = dic;

            #region 用户所属组已弃用

            //Dictionary<int, string> groudic = new Dictionary<int, string>();
            //var groups = CacheAccess.GetGroups(_groupContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            //foreach (var item in groups)
            //{
            //    groudic.Add(item.Id, item.GroupName);
            //}
            //ViewBag.groups = groudic;

            #endregion

            WorkTime workTime = new WorkTime();
            workTime.IsEnabled = false;
            ViewBag.WorkTime = workTime;
            return PartialView();
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Administrator admin, WorkTime workTime)
        {
            admin.IsLogin = true;
            admin.IsEnabled = true;
            string wtdArry = Request["wtdArry"];
            List<WorkTimeDetaile> wtdList = new List<WorkTimeDetaile>();
            if (!string.IsNullOrEmpty(wtdArry))
            {
                wtdList = JsonConvert.DeserializeObject<List<WorkTimeDetaile>>(wtdArry);
            }
            #region 自定义工作时间
            if (workTime.IsPersonalTime == true)
            {
                OperationResult oper = CheckWorkTime(workTime);
                if (oper.ResultType == OperationResultType.Success)
                {
                    string weekArry = Request["WorkWeek"];
                    admin.WorkTime = workTime;
                    admin.WorkTime.WorkWeek = weekArry;
                }
                else
                {
                    return Json(oper);
                }
            }
            admin.IsPersonalTime = workTime.IsPersonalTime;
            #endregion

            OperationResult resul = new OperationResult(OperationResultType.Error);

            int count = _administratorContract.Administrators.Where(x => x.MemberId == admin.MemberId).Count();
            if (count > 0)
            {
                resul = new OperationResult(OperationResultType.Error, "用户已经升级为员工");
            }
            count = _administratorContract.Administrators.Where(x => x.MacAddress == admin.MacAddress).Count();
            if (count > 0)
            {
                resul = new OperationResult(OperationResultType.Error, "Mac地址已经存在");
            }
            bool exis = _administratorContract.Administrators.Where(c => c.MemberId == admin.MemberId && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;
            if (exis)
            {
                resul = new OperationResult(OperationResultType.Error, "已存在同名的用户，操作失败");

            }
            else
            {
                string role = Request["role"];
                if (!string.IsNullOrEmpty(role))
                {
                    string[] rolear = role.Split(",");
                    List<int> roleids = new List<int>();
                    foreach (var item in rolear)
                    {
                        roleids.Add(Convert.ToInt32(item));
                    }
                    admin.Roles = _roleContract.Roles.Where(c => roleids.Contains(c.Id) && c.IsDeleted == false && c.IsEnabled == true).ToList();
                }

                admin.Member = _memberContract.Members.FirstOrDefault(c => c.Id == admin.MemberId);

                //给新用户初始化年假
                int jobPositionId = admin.JobPositionId ?? 0;
                JobPosition jobPos = _jobPositionContract.View(jobPositionId);
                AnnualLeave ann = jobPos.AnnualLeave.Children.Where(x => x.StartYear == 0).FirstOrDefault();
                Rest rest = new Rest()
                {
                    AnnualLeaveDays = 0,
                    ChangeRestDays = 0,
                    PaidLeaveDays = 0,
                    OperatorId = AuthorityHelper.OperatorId
                };
                if (ann != null)
                {
                    rest.AnnualLeaveDays = ann.Days;
                }
                admin.Rest = rest;
                admin.whetherChange = false;
                admin.whetherDateTime = DateTime.Now;
                resul = _administratorContract.Insert(admin);
                Administrator _admin = _administratorContract.Administrators.Where(x => x.MemberId == admin.MemberId).FirstOrDefault();
                _admin.Rest.AdminId = _admin.Id;
                resul = _administratorContract.Update(_admin);
                if (resul.ResultType == OperationResultType.Success)
                {
                    admin = _administratorContract.Administrators.Where(x => x.Id == _admin.Id).FirstOrDefault();
                    if (admin.IsPersonalTime)
                    {
                        if (admin.WorkTimeId.Value != 0)
                        {
                            int currentYear = DateTime.Now.Year;
                            int currentMonth = DateTime.Now.Month;
                            var wdListArry = new List<WorkTimeDetaile>();
                            foreach (var item in wtdList)
                            {
                                item.Id = 0;
                                item.WorkTimeId = admin.WorkTime != null ? admin.WorkTime.Id : 0;
                                item.Month = currentMonth;
                                item.Year = currentYear;
                                item.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                                item.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                                item.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                                item.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                                wdListArry.Add(item);
                            }
                            if (currentMonth == 12)
                            {
                                currentMonth = 1;
                                currentYear = currentYear + 1;
                            }
                            else
                            {
                                currentMonth = currentMonth + 1;
                            }
                            foreach (var item in wtdList)
                            {
                                item.Id = 0;
                                item.WorkTimeId = admin.WorkTime != null ? admin.WorkTime.Id : 0;
                                item.Month = currentMonth;
                                item.Year = currentYear;
                                item.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                                item.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                                item.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                                item.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                                wdListArry.Add(item);
                            }
                            resul = _workTimeDetaileContract.Insert(wdListArry.ToArray());
                            if (resul.ResultType != OperationResultType.Success)
                            {
                                return Json(resul);
                            }
                        }
                    }
                }
            }
            return Json(resul);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(Administrator dto, WorkTime worktime)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            string weekArry = Request["WorkWeek"];
            string wtdArry = Request["wtdArry"];
            List<WorkTimeDetaile> wtdList = new List<WorkTimeDetaile>();
            if (!string.IsNullOrEmpty(wtdArry))
            {
                wtdList = JsonConvert.DeserializeObject<List<WorkTimeDetaile>>(wtdArry);
            }
            int countName = _administratorContract.Administrators.Where(x => x.Id != dto.Id && x.Member.MemberName == dto.Member.MemberName).Count();
            if (countName > 0)
            {
                result.Message = "员工昵称已经存在";
                return Json(result);
            }
            worktime.WorkWeek = weekArry;
            result = this.CheckWorkTime(worktime);
            if (result.ResultType != OperationResultType.Success)
            {
                return Json(result);
            }
            Administrator admin = _administratorContract.Administrators.Where(c => c.Id == dto.Id).FirstOrDefault();
            bool whetherChange = false;

            if (admin.IsPersonalTime != worktime.IsPersonalTime)
            {
                if (admin.whetherChange != null && admin.whetherDateTime != null)
                {
                    TimeSpan ts = DateTime.Now - Convert.ToDateTime(admin.whetherDateTime);
                    TimeSpan ts_difference = new TimeSpan(7, 0, 0, 0);
                    whetherChange = ts > ts_difference;
                }
                if (admin.whetherChange == null || whetherChange)
                {
                    whetherChange = true;
                    admin.IsPersonalTime = worktime.IsPersonalTime;
                    admin.whetherDateTime = DateTime.Now;
                    admin.whetherChange = whetherChange;
                }
            }

            if (whetherChange)
            {
                if (worktime.IsPersonalTime)
                {
                    if (worktime.IsFlexibleWork)
                    {
                        worktime.PmStartTime = "00:00";
                        worktime.PmEndTime = "00:00";
                        worktime.AmStartTime = "00:00";
                        worktime.AmEndTime = "00:00";
                    }
                    if (admin.WorkTime != null)
                    {
                        admin.WorkTime.AmEndTime = string.IsNullOrEmpty(worktime.AmEndTime) ? "00:00" : Convert.ToDateTime(worktime.AmEndTime).ToShortTimeString();
                        admin.WorkTime.AmStartTime = string.IsNullOrEmpty(worktime.AmStartTime) ? "00:00" : Convert.ToDateTime(worktime.AmStartTime).ToShortTimeString();
                        admin.WorkTime.PmEndTime = string.IsNullOrEmpty(worktime.PmEndTime) ? "00:00" : Convert.ToDateTime(worktime.PmEndTime).ToShortTimeString();
                        admin.WorkTime.PmStartTime = string.IsNullOrEmpty(worktime.PmStartTime) ? "00:00" : Convert.ToDateTime(worktime.PmStartTime).ToShortTimeString();
                        admin.WorkTime.WorkWeek = weekArry;
                        //admin.WorkTime.IsPersonalTime = worktime.IsPersonalTime;
                        admin.WorkTime.IsFlexibleWork = worktime.IsFlexibleWork;
                        admin.WorkTime.WorkTimeName = worktime.WorkTimeName;
                        admin.WorkTime.IsVacations = worktime.IsVacations;
                        admin.WorkTime.WorkTimeType = worktime.WorkTimeType;
                        admin.WorkTime.WorkHour = worktime.WorkHour;
                        admin.WorkTime.Summary = worktime.Summary;
                    }
                    else
                    {
                        admin.WorkTime = worktime;
                        admin.WorkTime.AmEndTime = string.IsNullOrEmpty(worktime.AmEndTime) ? "00:00" : Convert.ToDateTime(worktime.AmEndTime).ToShortTimeString();
                        admin.WorkTime.AmStartTime = string.IsNullOrEmpty(worktime.AmStartTime) ? "00:00" : Convert.ToDateTime(worktime.AmStartTime).ToShortTimeString();
                        admin.WorkTime.PmEndTime = string.IsNullOrEmpty(worktime.PmEndTime) ? "00:00" : Convert.ToDateTime(worktime.PmEndTime).ToShortTimeString();
                        admin.WorkTime.PmStartTime = string.IsNullOrEmpty(worktime.PmStartTime) ? "00:00" : Convert.ToDateTime(worktime.PmStartTime).ToShortTimeString();
                        admin.WorkTime.WorkWeek = weekArry;
                    }
                    admin.WorkTime.IsPersonalTime = worktime.IsPersonalTime;
                }
                admin.IsPersonalTime = worktime.IsPersonalTime;
            }
            #region 更新数据
            admin.Member.MemberName = dto.Member.MemberName;
            if (!dto.Member.MemberPass.IsNullOrEmpty())
            {
                admin.Member.MemberPass = dto.Member.MemberPass.MD5Hash();
            }
            admin.Member.Email = dto.Member.Email;
            admin.Member.MobilePhone = dto.Member.MobilePhone;
            admin.Member.RealName = dto.Member.RealName;
            admin.Member.Gender = dto.Member.Gender;
            admin.DepartmentId = dto.DepartmentId;
            admin.JobPositionId = dto.JobPositionId;
            admin.EntryTime = dto.EntryTime;
            admin.MacAddress = dto.MacAddress;
            admin.Notes = dto.Notes;
            admin.Member.DateofBirth = dto.Member.DateofBirth;
            //admin.AdministratorTypeId = dto.AdministratorTypeId;
            #endregion

            admin.IsEnabled = true;


            #region 角色
            var rolestr = Request["role"];
            if (!string.IsNullOrEmpty(rolestr))
            {
                var rol = Request["role"].Split(',');
                List<int> roleIds = new List<int>();
                foreach (var item in rol)
                {
                    roleIds.Add(Convert.ToInt32(item));
                }
                List<int> allRoleId = new List<int>();
                if (admin.Roles != null)
                    allRoleId = admin.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();

                List<int> newAddRoleIds = roleIds.Where(c => !allRoleId.Contains(c)).ToList();
                List<int> removRoleIds = allRoleId.Where(c => !roleIds.Contains(c)).ToList();

                foreach (var item in newAddRoleIds)
                {
                    var addRole = _roleContract.Roles.Where(c => c.Id == item).FirstOrDefault();
                    if (admin.Roles == null)
                        admin.Roles = new List<Role>() { addRole };
                    else
                        admin.Roles.Add(addRole);
                }
                foreach (var item in removRoleIds)
                {
                    var removRole = admin.Roles.Where(c => c.Id == item).FirstOrDefault();
                    if (admin.Roles != null)
                        admin.Roles.Remove(removRole);
                }

            }
            else
            {
                admin.Roles.Clear();
            }

            #endregion

            //admin.Member = Utils.UpdateNavMemberInfo(admin, _memberContract);
            result = _administratorContract.Update(admin);
            if (result.ResultType == OperationResultType.Success)
            {
                admin = _administratorContract.Administrators.Where(x => x.Id == admin.Id).FirstOrDefault();
                if (admin.IsPersonalTime)
                {
                    int currentYear = DateTime.Now.Year;
                    int currentMonth = DateTime.Now.Month;
                    if (currentMonth == 12)
                    {
                        currentYear = currentYear + 1;
                        currentMonth = 1;
                    }
                    else
                    {
                        currentMonth = currentMonth + 1;
                    }
                    if (admin.WorkTime == null)
                    {
                        Random rd = new Random();
                        WorkTimeDto wtdto = new WorkTimeDto()
                        {
                            WorkTimeName = "工作时间" + rd.Next(99999999),
                            AdminId = admin.Id,
                            WorkTimeType = 0,
                            TimeType = 1,
                            IsPersonalTime = admin.IsPersonalTime,
                            IsVacations = false,
                            IsFlexibleWork = false,
                            AmStartTime = "0:00",
                            AmEndTime = "0:00",
                            PmStartTime = "0:00",
                            PmEndTime = "0:00",
                            WorkWeek = "",
                            WorkHour = 0,
                            Summary = ""
                        };
                        result = _workTimeContract.Insert(wtdto);
                        if (result.ResultType != OperationResultType.Success)
                        {
                            return Json(result);
                        }
                        admin.WorkTime = _workTimeContract.WorkTimes.Where(x => x.AdminId == admin.Id).First();
                    }
                    int countDay = admin.WorkTime != null ? _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id
    && x.Year == currentYear && x.Month == currentMonth).Count() : 0;
                    int totalNumber = admin.WorkTime != null ? _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id).Count() : 0;
                    if ((countDay == 0 || countDay == 31) && totalNumber == 62)
                    {
                        var dtoArry = new List<WorkTimeDetaileDto>();
                        foreach (var item in wtdList)
                        {
                            WorkTimeDetaileDto wtddto = new WorkTimeDetaileDto();
                            wtddto.Id = item.Id;
                            wtddto.WorkTimeId = admin.WorkTime.Id;
                            wtddto.IsFlexibleWork = item.IsFlexibleWork;
                            wtddto.WorkDay = item.WorkDay;
                            wtddto.WorkHour = item.WorkHour;
                            wtddto.WorkTimeType = item.WorkTimeType;
                            wtddto.Year = currentYear;
                            wtddto.Month = currentMonth;
                            if (item.IsFlexibleWork || wtddto.WorkTimeType == 2)
                            {
                                wtddto.PmStartTime = "00:00";
                                wtddto.PmEndTime = "00:00";
                                wtddto.AmStartTime = "00:00";
                                wtddto.AmEndTime = "00:00";
                            }
                            else
                            {
                                wtddto.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                                wtddto.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                                wtddto.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                                wtddto.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                            }
                            dtoArry.Add(wtddto);
                        }
                        result = _workTimeDetaileContract.Update(dtoArry.ToArray());
                        if (result.ResultType != OperationResultType.Success)
                        {
                            return Json(result);
                        }
                    }
                    else
                    {
                        int wtdC = admin.WorkTime != null ? _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id).Count() : 0;
                        if (wtdC > 0)
                        {
                            result = _workTimeDetaileContract.TrueRemove(currentYear, currentMonth, admin.WorkTime.Id);
                            if (result.ResultType != OperationResultType.Success)
                            {
                                return Json(result);
                            }
                        }
                        if (admin.WorkTime != null && admin.WorkTime.Id != 0)
                        {
                            List<WorkTimeDetaile> listArry = new List<WorkTimeDetaile>();
                            foreach (var item in wtdList)
                            {
                                WorkTimeDetaile wd = new WorkTimeDetaile();
                                wd.Id = 0;
                                wd.WorkTimeId = admin.WorkTime.Id;
                                wd.IsFlexibleWork = item.IsFlexibleWork;
                                wd.WorkDay = item.WorkDay;
                                wd.WorkHour = item.WorkHour;
                                wd.WorkTimeType = item.WorkTimeType;
                                wd.Year = currentYear;
                                wd.Month = currentMonth;
                                wd.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                                wd.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                                wd.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                                wd.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                                listArry.Add(wd);
                            }
                            int currentCount = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id
                  && x.Year == DateTime.Now.Year && x.Month == DateTime.Now.Month).Count();
                            if (currentCount == 0)
                            {
                                foreach (var item in wtdList)
                                {
                                    WorkTimeDetaile wd = new WorkTimeDetaile();
                                    wd.Id = 0;
                                    wd.WorkTimeId = admin.WorkTime.Id;
                                    wd.IsFlexibleWork = item.IsFlexibleWork;
                                    wd.WorkDay = item.WorkDay;
                                    wd.WorkHour = item.WorkHour;
                                    wd.WorkTimeType = item.WorkTimeType;
                                    wd.Year = currentYear;
                                    wd.Month = currentMonth;
                                    wd.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                                    wd.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                                    wd.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                                    wd.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                                    listArry.Add(wd);
                                }
                                result = _workTimeDetaileContract.Insert(listArry.ToArray());
                            }

                            if (result.ResultType != OperationResultType.Success)
                            {
                                return Json(result);
                            }
                        }
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {

            Administrator admi = _administratorContract.Administrators.Where(c => c.Id == Id).FirstOrDefault();

            List<int> adRole = new List<int>();
            List<int> adgroup = new List<int>();
            string isChange = "0";
            if (admi != null)
            {
                if (admi.whetherChange != null && admi.whetherDateTime != null)
                {
                    if ((DateTime.Now - admi.whetherDateTime.Value).TotalDays > 6)
                    {
                        isChange = "1";
                    }
                }
                else
                {
                    isChange = "1";
                }
            }
            Dictionary<int, string[]> roles = new Dictionary<int, string[]>();
            if (admi != null)
            {
                adRole = admi.Roles.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();
                //adgroup = admi.Groups.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => c.Id).ToList();
            }
            var roleLis = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            foreach (var item in roleLis)
            {
                string selected = adRole.Contains(item.Id) ? "1" : "0";
                roles.Add(item.Id, new string[] { item.RoleName, selected });
            }
            ViewBag.roles = roles;
            ViewBag.isChange = isChange;
            #region 用户所属组已弃用

            //Dictionary<int, string[]> groups = new Dictionary<int, string[]>();

            //var groupLis = CacheAccess.GetGroups(_groupContract).Where(c => c.IsDeleted == false && c.IsEnabled == true);
            //foreach (var item in groupLis)
            //{
            //    string selected = adgroup.Contains(item.Id) ? "1" : "0";
            //    groups.Add(item.Id, new string[] { item.GroupName, selected });
            //}
            //ViewBag.groups = groups;

            #endregion

            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_departmentContract, false);
            //ViewBag.AdministratorTypes = CacheAccess.GetAdministratorTypeListItem(AdministratorTypeContract, false).Take(1).ToList();
            var result = _administratorContract.View(Id);
            var jobPos = _jobPositionContract.SelectList(string.Empty, result.DepartmentId ?? 0);
            ViewBag.JobPositions = jobPos;
            WorkTime workTime = new WorkTime()
            {
                AmStartTime = "0:0",
                AmEndTime = "0:0",
                PmStartTime = "0:0",
                PmEndTime = "0:0",
                IsEnabled = false,
                Id = 0
            };
            if (admi.WorkTime != null && admi.IsPersonalTime)
            {
                workTime = admi.WorkTime;
            }
            ViewBag.WorkTime = workTime;

            //WarhouseAll(Id, admi);
            return PartialView(result);
        }

        public JsonResult GetWtdArry(int workTimeId)
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            var WtdArry = new List<WorkTimeDetaile>();
            var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimeId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
            if (monthList.Count == 2)
            {
                int month = monthList.Min();
                int minYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimeId && x.Month == month).Year;
                int maxMonth = monthList.Max();
                int maxYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimeId && x.Month == maxMonth).Year;
                //if (monthList.Contains(currentMonth))
                //{
                if ((month == currentMonth && minYear == currentYear) || (maxMonth == currentMonth && maxYear == currentYear))
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
                            currentYear = minYear;
                        }
                        else
                        {//若当月为1月，且未进行下个月的排班，则下月用最小月（此处即maxMonth）排班
                            month = maxMonth;
                            currentYear = maxYear;
                        }
                    }
                    else
                    {
                        if (maxMonth > currentMonth)
                        {
                            month = maxMonth;
                        }
                    }
                }
                else
                {
                    if (minYear > maxYear)
                    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                        month = maxMonth;
                        currentYear = maxYear;
                    }else
                    {
                        currentYear = minYear; 
                    }
                }
                WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimeId && x.Year == currentYear
           && x.Month == month).ToList();
            }
            else
            {
                if (currentMonth == 12)
                {
                    currentYear = currentYear + 1;
                    currentMonth = 1;
                }
                else
                {
                    currentMonth = currentMonth + 1;
                }
                WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimeId && x.Year == currentYear
                && x.Month == currentMonth).ToList();
            }
            var arry = WtdArry.Select(x => new
            {
                x.Id,
                x.IsFlexibleWork,
                x.PmEndTime,
                x.PmStartTime,
                x.WorkDay,
                x.WorkHour,
                x.WorkTimeId,
                x.WorkTimeType,
                x.AmStartTime,
                x.AmEndTime
            }).ToList();
            return Json(arry);
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _administratorContract.View(Id);
            Administrator admi = _administratorContract.Administrators.Where(c => c.Id == Id).FirstOrDefault();
            if (admi != null)
            {
                if (admi.Roles != null)
                {
                    ViewBag.roles = admi.Roles.Where(c => c.IsDeleted == false).Select(c => c.RoleName).ToList();
                }
                else
                    ViewBag.roles = null;
            }

            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                //var query = _administratorContract.Administrators.Where(w => w.AdministratorTypeId != 3);//不是设计师
                var query = _administratorContract.Administrators.Where(w => w.DepartmentId.HasValue && (w.Department.DepartmentType == DepartmentTypeFlag.公司 || w.Department.DepartmentType == DepartmentTypeFlag.店铺));
                var list = query.Where<Administrator, int>(predicate, request.PageCondition, out count).ToList();

                Dictionary<Administrator, List<Administrator>> li = GetTreeList(list);
                var lis = new List<object>();
                foreach (var item in li)
                {
                    var m = item.Key;
                    lis.Add(new
                    {
                        MemberName = string.Empty,
                        MemberPass = string.Empty,
                        Email = string.Empty,
                        MobilePhone = string.Empty,
                        RealName = string.Empty,
                        Gender = 0,
                        UniquelyIdentifies = string.Empty,
                        m.Notes,
                        m.LoginCount,
                        m.LoginTime,
                        Id = "p" + m.Id,
                        ParentId = "",
                        DepartmentName = m.Notes,
                        m.DepartmentId,
                        m.IsDeleted,
                        m.IsEnabled,
                        m.Sequence,
                        m.UpdatedTime,
                        m.CreatedTime,
                        TypeName = string.Empty,
                        Stores = m.Stores.Where(x => x.IsEnabled && !x.IsDeleted).Select(s => s.StoreName),
                        //Storages = m.Storages.Where(x => x.IsEnabled && !x.IsDeleted).Select(x => x.StorageName),
                        Roles = m.Roles.Where(x => x.IsEnabled && !x.IsDeleted).Select(r => r.RoleName),
                        JobPositionName = string.Empty
                    });
                    var child = item.Value.Select(c => new
                    {
                        c.Member.MemberName,
                        c.Member.MemberPass,
                        c.Member.Email,
                        c.Member.MobilePhone,
                        c.Member.RealName,
                        c.Member.Gender,
                        c.Member.UniquelyIdentifies,
                        c.Notes,
                        c.LoginCount,
                        c.LoginTime,
                        c.Id,
                        ParentId = "p" + m.Id,
                        DepartmentName = "",
                        c.DepartmentId,
                        c.IsDeleted,
                        c.IsEnabled,
                        c.Sequence,
                        c.UpdatedTime,
                        c.CreatedTime,
                        //c.AdministratorType.TypeName,
                        Stores = c.Stores.Where(x => x.IsEnabled && !x.IsDeleted).Select(s => s.StoreName),
                        Roles = c.Roles.Where(x => x.IsEnabled && !x.IsDeleted).Select(r => r.RoleName),
                        //Storages = c.Storages.Where(x => x.IsEnabled && !x.IsDeleted).Select(x => x.StorageName),
                        c.JobPosition.JobPositionName
                    });
                    lis.AddRange(child);
                }
                return new GridData<object>(lis, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStorages(int Id)
        {
            ViewBag.AdminId = Id;
            return PartialView();
        }

        public ActionResult GetStoragesInfo(int Id)
        {
            var admin = _administratorContract.Administrators.Where(x => x.Id == Id && x.IsEnabled && !x.IsDeleted).FirstOrDefault();
            List<object> list = new List<object>();
            if (admin != null)
            {
                var liststorages = PermissionHelper.ManagedStorages(admin.Id, _administratorContract, s => new { storageName = s.StorageName }, f => f.IsEnabled && !f.IsDeleted);
                list.AddRange(liststorages);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRoles(int Id)
        {
            ViewBag.AdminId = Id;
            return PartialView();
        }

        public ActionResult GetRolesInfo(int Id)
        {
            var admin = _administratorContract.Administrators.Where(x => x.Id == Id && x.IsEnabled && !x.IsDeleted).FirstOrDefault();
            List<object> list = new List<object>();
            if (admin != null)
            {
                foreach (var item in admin.Roles.Where(x => x.IsEnabled && !x.IsDeleted))
                {
                    var da = new
                    {
                        RoleName = item.RoleName
                    };
                    list.Add(da);
                }
                list = list.Distinct().ToList();
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        private Dictionary<Administrator, List<Administrator>> GetTreeList(List<Administrator> li)
        {
            Dictionary<Administrator, List<Administrator>> dic = new Dictionary<Administrator, List<Administrator>>();
            foreach (var item in li)
            {
                var pro = item.GetType().GetProperty("DepartmentId");
                int partId = Convert.ToInt32(pro.GetValue(item));
                var part = _departmentContract.Departments.Where(c => c.Id == partId && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (part != null)
                {
                    Administrator ad = new Administrator()
                    {
                        Id = part.Id,
                        Notes = part.DepartmentName,
                        DepartmentId = part.Id
                    };

                    if (dic.Keys.Where(c => c.Id == ad.Id).Count() == 0)
                    {
                        dic.Add(ad, new List<Administrator>() { item });
                    }
                    else
                    {
                        List<Administrator> chils = dic.Where(c => c.Key.Id == ad.Id).FirstOrDefault().Value;

                        chils.Add(item);
                        var key = dic.Where(c => c.Key.Id == ad.Id).FirstOrDefault().Key;
                        dic[key] = chils;

                    }
                }
            }
            return dic;
        }


        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _administratorContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _administratorContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _administratorContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _administratorContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _administratorContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _administratorContract.Administrators.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _administratorContract.Administrators.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 在修改密码时判断原始密码是否正确
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExistPwd()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            string id = Request["id"];
            string pwd = Request["pwd"];
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(pwd))
            {
                pwd = pwd.MD5Hash();
                int _id = Convert.ToInt32(id);
                bool exis = _administratorContract.Administrators.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == _id && c.Member.MemberPass == pwd).Count() > 0;
                if (exis)
                {
                    resul = new OperationResult(OperationResultType.Success);
                }
            }
            else
            {
                resul = new OperationResult(OperationResultType.Error);
            }
            return Json(resul);

        }

        public JsonResult GetAdminInfo(int[] ids)
        {
            var admins = _administratorContract.Administrators.Select(a => new
            {
                id = a.Id,
                name = a.Member.RealName
            });
            System.Collections.Generic.List<object> li = new List<object>();
            foreach (var admin in admins.ToArray())
            {
                if (Array.IndexOf(ids, admin.id) > -1)
                {
                    li.Add(new { id = admin.id, name = admin.name });
                }
            }
            return Json(li, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取全部会员
        /// </summary>
        /// <returns></returns>
        public ActionResult MembList()
        {
            List<int?> stores = _memberContract.Members.Select(c => c.StoreId).Distinct().ToList();

            var li = _storeContract.Stores.Where(c => c.IsDeleted == false && c.IsEnabled == true && stores.Contains(c.Id)).Select(c => new SelectListItem()
            {
                Text = c.StoreName,
                Value = c.Id.ToString()
            }).ToList();
            li.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = ""
            });
            ViewBag.StoreIds = li;
            var colls = _collocationContract.Collocations.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new SelectListItem()
            {
                Value = c.Id.ToString(),
                Text = c.CollocationName,
            }).ToList();
            colls.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = ""
            });
            ViewBag.CollocationIds = colls;
            return PartialView();
        }
        //yxk 2015-11
        public ActionResult GetInfoById(int Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var memb = _memberContract.Members.Where(c => c.Id == Id).Select(c => new
            {

                c.Id,
                MembNum = c.UniquelyIdentifies,
                c.RealName,
                c.MobilePhone,
                c.MemberName,
                c.MemberPass,
                c.CardNumber,
                c.IDCard,
                c.Email,
                c.Gender,

            }).FirstOrDefault();
            //判断将要添加的员工是否已存在
            bool exis = _administratorContract.Administrators.Where(c => (c.Member.MemberName == memb.MemberName || c.Member.UniquelyIdentifies == memb.MembNum) && c.IsDeleted == false && c.IsEnabled == true).Count() > 0;

            if (exis)
            {
                resul = new OperationResult(OperationResultType.Error, "员工编号或者昵称已经存在") { Data = memb };
            }
            else
            {
                resul = new OperationResult(OperationResultType.Success, "") { Data = memb };
            }
            return Json(resul);

        }

  

        #region 获取部门下的职位
        public JsonResult GetJobPosition()
        {
            string strDepartmentId = Request["DepartmentId"];
            int departmentId = 0;
            if (!string.IsNullOrEmpty(strDepartmentId))
            {
                departmentId = int.Parse(strDepartmentId);
            }
            var listEntity = _jobPositionContract.JobPositions.Where(x => x.DepartmentId == departmentId);
            var entity = listEntity.Select(x => new
            {
                x.Id,
                x.JobPositionName,
            }).ToList();
            return Json(entity, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 校验工作时间
        /// <summary>
        /// 校验工作时间
        /// </summary>
        /// <param name="workTime"></param>
        /// <returns></returns>
        private OperationResult CheckWorkTime(WorkTime workTime)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            if (workTime.IsPersonalTime == true)
            {
                if (string.IsNullOrEmpty(workTime.WorkWeek))
                {
                    oper.Message = "请选择工作时间";
                    return oper;
                }
            }
            oper.ResultType = OperationResultType.Success;
            return oper;
        }
        #endregion

        #region 批量配置

        public ActionResult BatchConfig()
        {
            var roleLis = CacheAccess.GetRoles(_roleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(s => new SelectListItem()
            {
                Value = s.Id + "",
                Text = s.RoleName
            }).ToList();
            ViewBag.roles = roleLis;

            return PartialView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AdminIds">选择的员工</param>
        /// <param name="RoleIds">选择的角色</param>
        /// <param name="IsAppend">true附加,false替换角色</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BatchConfig(int[] AdminIds, int[] RoleIds, bool IsAppend = true)
        {
            var res = new OperationResult(OperationResultType.ValidError);
            if (AdminIds == null || RoleIds == null)
            {
                res.Message = "请选择员工和角色";
            }
            else
            {
                var listadmin = _administratorContract.Administrators.Where(w => w.IsEnabled && !w.IsDeleted && AdminIds.Contains(w.Id)).Include(c => c.Roles).ToList();
                var roles = _roleContract.Roles.Where(w => w.IsEnabled && !w.IsDeleted && RoleIds.Contains(w.Id)).ToList();
                foreach (var admin in listadmin)
                {
                    var orgRoles = admin.Roles.ToList();
                    if (IsAppend)
                    {
                        var appendRoles = roles.Except(orgRoles);
                        foreach (var item in appendRoles)
                        {
                            admin.Roles.Add(item);
                        }
                    }
                    else
                    {
                        admin.Roles.Clear();
                        admin.Roles = roles;
                    }
                }

                res = _administratorContract.Update(listadmin.ToArray());
            }
            return Json(res);
        }

        #endregion

        #region 选择员工

        public ActionResult VAdmin()
        {
            return PartialView();
        }

        public ActionResult AdminList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var count = 0;
            var list = (from s in _administratorContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.Member.MemberName,
                            s.Member.MobilePhone,
                            //s.Member.UserPhoto,
                            Gender = s.Member.Gender == 1 ? "男" : "女",
                            s.Member.RealName,
                            s.CreatedTime,
                            s.JobPosition.JobPositionName,
                            s.Department.DepartmentName,

                        }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetAdminSelectList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var list = _administratorContract.Administrators.Where(predicate).Select(m => new
                {
                    m.Id,
                    //m.Member.MemberName,
                    m.Member.RealName
                }).ToList();
                return list;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
