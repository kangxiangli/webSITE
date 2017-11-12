using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.ERP.Services.Implements;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Offices;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Authorities;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Authority;
using System.Text.RegularExpressions;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.Web.Extensions;
using Whiskey.ZeroStore.ERP.Services;
using System.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    //[License(CheckMode.Verify)]
    [AllowCross]
    public class AdminController : Controller
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AdminLoginController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IHolidayContract _holidayContract;

        protected readonly IAttendanceStatisticsContract _attStatisticsContract;

        protected readonly IMemberDepositContract _memberDepositContract;

        protected readonly IMemberConsumeContract _memberConsumContract;

        protected readonly IResignationContract _resignationContract;
        private readonly IStoreStatisticsContract _statContract;
        protected readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        protected readonly IMsgNotificationContract _msgNotificationContract;
        protected readonly IMessagerContract _messagerContract;
        protected readonly IWorkOrderDealtWithContract _workOrderDealtWithContract;
        protected readonly IExamRecordContract _examRecordContract;
        public AdminController(IAdministratorContract administratorContract,
           IAttendanceContract attendanceContract,
            IMemberContract memberContract,
            IDepartmentContract departmentContract,
            IHolidayContract holidayContract,
            IAttendanceStatisticsContract attStatisticsContract,
            IResignationContract resignationContract,
            IStoreStatisticsContract statContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IMemberDepositContract memberDepositContract,
            IMemberConsumeContract memberConsumContract,
            IMsgNotificationContract msgNotificationContract,
            IMessagerContract messagerContract,
            IWorkOrderDealtWithContract workOrderDealtWithContract,
            IExamRecordContract examRecordContract)
        {
            _administratorContract = administratorContract;
            _attendanceContract = attendanceContract;
            _memberContract = memberContract;
            _departmentContract = departmentContract;
            _holidayContract = holidayContract;
            _attStatisticsContract = attStatisticsContract;
            _resignationContract = resignationContract;
            _statContract = statContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _memberDepositContract = memberDepositContract;
            _memberConsumContract = memberConsumContract;
            _msgNotificationContract = msgNotificationContract;
            _messagerContract = messagerContract;
            _workOrderDealtWithContract = workOrderDealtWithContract;
            _examRecordContract = examRecordContract;
        }
        #endregion

        #region 获取签到数据
        /// <summary>
        /// 获取签到数据
        /// </summary>
        /// <returns></returns>
        public JsonResult SignInfo()
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "请重新登陆");
                int adminId = 0;
                int IsFlexibleWork = 0;
                string strMacAdd = Request["MacAdd"];

                string strAdminId = Request["AdminId"];
                if (string.IsNullOrEmpty(strAdminId) || !int.TryParse(strAdminId, out adminId))
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                int days = 7;
                Administrator admin = _administratorContract.Administrators.FirstOrDefault(x => x.Id == adminId);
                oper = _attendanceContract.GetList(adminId, days);
                if (oper.ResultType != OperationResultType.Success)
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                WorkTime workTime = new WorkTime();
                if (admin.IsPersonalTime)
                {
                    if (admin.WorkTime != null)
                    {

                        int currentYear = DateTime.Now.Year;
                        int currentMonth = DateTime.Now.Month;
                        var WtdArry = new List<WorkTimeDetaile>();
                        var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTimeId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
                        if (monthList.Count() < 2)
                        {
                            oper = new OperationResult(OperationResultType.Error, "没有排班信息");
                            return Json(oper, JsonRequestBehavior.AllowGet);
                        }
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

                        var _workDetaile = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId
                        && x.Year == currentYear && x.Month == currentMonth && x.WorkDay == DateTime.Now.Day);
                        if (_workDetaile == null)
                        {
                            oper = new OperationResult(OperationResultType.Error, "今天不是工作日");
                            return Json(oper, JsonRequestBehavior.AllowGet);
                        }
                        workTime.IsFlexibleWork = false;
                        workTime.IsPersonalTime = true;
                        if (_workDetaile.WorkTimeType != 2)
                        {
                            workTime.AmStartTime = _workDetaile.AmStartTime;
                            workTime.PmEndTime = _workDetaile.PmEndTime;
                        }
                        else
                        {
                            workTime.AmStartTime = "00:00";
                            workTime.PmEndTime = "00:00";
                        }
                    }
                }
                else
                {
                    workTime = admin.JobPosition.WorkTime;
                    if (admin.JobPosition.WorkTime.IsFlexibleWork)
                    {
                        workTime.AmStartTime = "";
                        workTime.AmEndTime = "";
                        workTime.PmStartTime = "";
                        workTime.PmEndTime = "";
                        IsFlexibleWork = 1;
                    }
                }
                //WorkTime workTime = admin.JobPosition.WorkTime;
                string strAmStartTime = workTime.AmStartTime;
                string strAmEndTime = workTime.AmEndTime;
                string strPmStartTime = workTime.PmStartTime;
                string strPmEndTime = workTime.PmEndTime;
                if (workTime.IsFlexibleWork == true)
                {
                    strAmStartTime = "9:00";
                    strAmEndTime = "12:00";
                    strPmStartTime = "13:00";
                    strPmEndTime = "18:00";
                    workTime.WorkHour = 8;
                }
                int minutes = 0;
                if (!string.IsNullOrEmpty(strAmEndTime) && !string.IsNullOrEmpty(strPmStartTime))
                {
                    DateTime amEndTime = DateTime.Parse(strAmEndTime);
                    DateTime pmStartTime = DateTime.Parse(strPmStartTime);
                    minutes = (pmStartTime - amEndTime).Minutes;
                }
                DateTime nowTime = DateTime.Now;
                List<Attendance> listAtt = _attendanceContract.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AttendanceTime.Year == nowTime.Year && x.AttendanceTime.Month == nowTime.Month && x.AttendanceTime.Day == nowTime.Day).ToList();
                Attendance att = listAtt.FirstOrDefault(x => x.AdminId == adminId);
                string strSequence = string.Empty;
                bool isLogin = false;
                if (att != null)
                {
                    int index = listAtt.IndexOf(att) + 1;
                    strSequence = "第" + index.ToString() + "名";
                    isLogin = true;
                }
                //listAtt.IndexOf()
                var res = CheckMac(strMacAdd);
                string strDepartmentName = "未知打卡环境";
                if (res.ResultType == OperationResultType.Success)
                {
                    Department depart = res.Data as Department;
                    strDepartmentName = depart.DepartmentName;
                }

                var date = DateTime.Now.Date;//今天
                var dateye = date.AddDays(-1);//昨天

                var saletop1 = _statContract.StoreStatistics.Where(w => w.CreatedTime < date && w.CreatedTime >= dateye).GroupBy(g => new { g.StoreId, g.Store.DepartmentId }).Select(s => new { StoreId = s.Key.StoreId, DepartId = s.Key.DepartmentId, RetailAmount = s.Sum(ss => ss.RetailAmount + ss.MemberRechargeBalanceAmount) })
                                    .OrderByDescending(o => o.RetailAmount).FirstOrDefault();//昨天销售额第一的店铺

                int WhetherToWork = GetUserSameDayWorkTimeInfo(adminId);
                var entity = new
                {
                    UserPhoto = admin.Member == null ? "/Content/Images/logo-_03.png" : (string.IsNullOrEmpty(admin.Member.UserPhoto) ? "/Content/Images/logo-_03.png" : admin.Member.UserPhoto),
                    IsLogin = isLogin,
                    StartTime = DateTime.Parse(strAmStartTime).ToString("yyyy/MM/dd HH:mm:ss"),
                    EndTime = DateTime.Parse(strPmEndTime).ToString("yyyy/MM/dd HH:mm:ss"),
                    RestMinutes = minutes,
                    Sequence = strSequence,
                    DepartmentName = strDepartmentName,
                    admin.Member.RealName,
                    CurrentDate = DateTime.Now.ToString("yyyy/MM/dd"),
                    AttenInfo = oper.Data,
                    MyStoreIsTop = saletop1?.DepartId == admin.DepartmentId,
                    SaleTopStoreId = saletop1?.StoreId,
                    IsFlexibleWork = IsFlexibleWork,
                    WhetherToWork = WhetherToWork
                };
                oper.Data = entity;

                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        #region 签到
        /// <summary>
        /// 签到
        /// </summary>
        /// <returns></returns>
        public JsonResult LoginIn(string AdminId, string MacAddress)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请重新登陆");
            int adminId = 0;
            string strAdminId = AdminId;
            if (string.IsNullOrEmpty(strAdminId))
            {
                return Json(oper);
            }

            adminId = int.Parse(strAdminId);

            if (ExistUnreadMsg(adminId))
            {
                oper.Message = "请先阅读通知";
                return Json(oper);
            }

            var examRecords = _examRecordContract.Entities.Count(r => r.AdminId == adminId && !r.IsDeleted && r.IsEnabled && r.EntryTrainStatus == 0);
            if (examRecords > 0)
            {
                oper.Message = "您的入职培训试题未完成，无法签到";
                return Json(oper);
            }

            if (_workOrderDealtWithContract.Entities.Count(d => d.HandlerID == adminId && d.Status == 0) > 0)
            {
                oper.Message = "您有未处理的配单信息";
                return Json(oper);
            }

            var dic = _attendanceContract.DoubleScoreReminderBySign(adminId);

            if (dic["DeductionDoubleScore"])
            {
                oper.Message = "扣除双倍积分";
                oper.Other = -2;
                return Json(oper);
            }

            Administrator admin = _administratorContract.View(adminId);

            var checkmac = admin?.JobPosition.CheckMac != false;

            if (checkmac)
            {
                string strMac = MacAddress;
                oper = this.CheckMac(strMac);

                if (oper.ResultType != OperationResultType.Success) return Json(oper, JsonRequestBehavior.AllowGet);
            }
            Department department = oper.Data as Department;
            oper = _attendanceContract.LoginIn(adminId, department, !checkmac);
            if (dic["IsReminder"])
            {
                oper.Other = -1;
            }
            else
            {
                oper.Other = 0;
            }
            admin.UpdatedTime = DateTime.Now;
            admin.LoginTime = DateTime.Now;
            _administratorContract.Update(admin);

            var res = _statContract.SetStoreOpenWhenFirstSignIn(adminId);
            if (res.ResultType != OperationResultType.Success)
            {
                _Logger.Error(res.Message);
            }
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
        public JsonResult LoginOut(string AdminId, string MacAddress)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请重新登陆");
            int adminId = 0;
            string strAdminId = AdminId;
            if (string.IsNullOrEmpty(strAdminId))
            {
                return Json(oper);
            }
            adminId = int.Parse(strAdminId);
            if (ExistUnreadMsg(adminId))
            {
                oper.Message = "请先阅读通知";
                return Json(oper);
            }
            Administrator admin = _administratorContract.View(adminId);

            var checkmac = admin?.JobPosition.CheckMac != false;

            if (checkmac)
            {
                string strMac = MacAddress;
                oper = this.CheckMac(strMac);

                if (oper.ResultType != OperationResultType.Success) return Json(oper, JsonRequestBehavior.AllowGet);
            }

            Department department = oper.Data as Department;
            oper = _attendanceContract.LoginOut(adminId, department, !checkmac);
            var res = _statContract.StatStoreWhenSignOut(adminId);
            if (res.ResultType != OperationResultType.Success)
            {
                _Logger.Error(res.Message);
            }
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 退出登录
        public JsonResult Logout()
        {
            FormsAuthentication.SignOut();
            return Json(new OperationResult(OperationResultType.Success));
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
            OperationResult oper = new OperationResult(OperationResultType.Error, "未知打卡环境");
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

        #region 获取打卡环境
        private string GetDepartmentName(string strMacAdd)
        {
            IQueryable<Department> listDepartment = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            string strDepartmentName = "未知打卡环境";
            if (string.IsNullOrEmpty(strMacAdd))
            {
                return strDepartmentName;
            }

            foreach (Department depart in listDepartment)
            {
                if (!string.IsNullOrEmpty(depart.MacAddress))
                {
                    bool match = depart.MacAddress.IsMatch(strMacAdd);
                    if (match)
                    {
                        strDepartmentName = depart.DepartmentName;
                    }
                }
            }
            return strDepartmentName;
        }
        #endregion

        #region 算出工作时间
        private int GetWorkDay(DateTime date, WorkTime workTime)
        {
            string strDateIndex = date.ToString("yyyy/MM/01");
            DateTime dateIndex = DateTime.Parse(strDateIndex);
            int index = DateTime.DaysInMonth(date.Year, date.Month);
            int workDay = 0;
            string[] arrWeeks = workTime.WorkWeek.Split(",");
            Dictionary<string, int> dic = new Dictionary<string, int>();
            if (workTime.IsVacations == true)
            {
                dic = _holidayContract.GetHoliday();
            }
            StringBuilder sbWeekDay = new StringBuilder();
            StringBuilder sbCurrentDate = new StringBuilder();
            while (true)
            {
                if (index <= 0)
                {
                    break;
                }
                else
                {
                    sbWeekDay.Append(dateIndex.DayOfWeek.ToString("d"));
                    sbCurrentDate.Append(dateIndex.ToString("yyyyMMdd"));
                    if (dic != null)
                    {
                        int count = dic.Where(x => x.Key == sbCurrentDate.ToString() && x.Value == (int)WorkDateFlag.WorkDay).Count();
                        if (count > 0)
                        {
                            workDay += 1;
                        }
                    }
                    else
                    {
                        if (arrWeeks.Contains(sbWeekDay.ToString()))
                        {
                            workDay += 1;
                        }
                    }
                }
                dateIndex.AddDays(1);
                index -= 1;
                sbWeekDay.Clear();
                sbCurrentDate.Clear();
            }
            return workDay;
        }
        #endregion


        #region 获取联系人列表
        /// <summary>
        /// 获取联系人列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetContacts(int pageIndex, int pageSize, string KeyWord)
        {
            string strKeyWord = KeyWord;
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功");
            List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            List<Administrator> list = new List<Administrator>();
            var listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true && !string.IsNullOrEmpty(x.UniquelyIdentifies));
            if (!string.IsNullOrEmpty(strKeyWord))
            {
                listAdmin = listAdmin.Where(x => x.Member.RealName.Contains(strKeyWord) || (!string.IsNullOrEmpty(x.Member.MobilePhone) && x.Member.MobilePhone.Contains(strKeyWord))).ToList();
            }
            int count = listAdmin.Count();
            var data = listAdmin.OrderBy(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(x => new
            {
                UserPhoto = x.Member.UserPhoto,
                MemberName = string.IsNullOrEmpty(x.Member.MemberName) ? string.Empty : x.Member.MemberName,
                JobPositionName = x.JobPosition == null ? string.Empty : x.JobPosition.JobPositionName,
                MobilePhone = string.IsNullOrEmpty(x.Member.MobilePhone) ? string.Empty : x.Member.MobilePhone,
                total = count
            }).ToList();
            oper.Data = data;
            oper.Other = count;
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取部门列表
        [HttpPost]
        public JsonResult GetDepartmentList()
        {
            List<Department> listDepart = _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
            IQueryable<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId != null);
            IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<AdminInfo> listAdminInfo = (from ad in listAdmin
                                             join
                                             me in listMember
                                             on
                                             ad.Member.UniquelyIdentifies equals me.UniquelyIdentifies
                                             select new AdminInfo
                                             {
                                                 AdminName = ad.Member.RealName,
                                                 DepartId = ad.DepartmentId ?? 0,
                                                 Id = ad.Id,
                                                 JobPositonName = ad.JobPosition == null ? string.Empty : ad.JobPosition.JobPositionName,
                                                 PhoneNum = ad.Member.MobilePhone,
                                                 UserPhoto = me.UserPhoto,
                                             }).ToList();
            List<DepartInfo> listDepartInfo = new List<DepartInfo>();
            foreach (Department depart in listDepart)
            {
                DepartInfo departInfo = new DepartInfo()
                {
                    Id = depart.Id,
                    DepartName = depart.DepartmentName,
                };
                departInfo.Admins = listAdminInfo.Where(x => x.DepartId == depart.Id).ToList();
                listDepartInfo.Add(departInfo);
            }
            return Json(listDepartInfo);
        }
        #endregion

        #region 获取员工基本信息
        [HttpPost]
        public JsonResult GetAdminInfo(int AdminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (AdminId <= 0)
            {
                return Json(OperationResult.Error("参数错误"));
            }
            Administrator admin = _administratorContract.View(AdminId);
            if (admin == null)
            {
                oper.Message = "员工不存在";
                return Json(oper);
            }
            else
            {
                Member member = _memberContract.Members.FirstOrDefault(x => x.Id == admin.MemberId);
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == admin.DepartmentId).ToList();
                string strLeader = string.Empty;
                string strUserPhoto = string.Empty;
                string strDateofBirth = string.Empty;
                string strDepartName = admin.DepartmentId == null ? string.Empty : admin.Department.DepartmentName;
                string strJobName = admin.JobPositionId == null ? string.Empty : admin.JobPosition.JobPositionName;
                string IDCard = string.Empty;
                if (member != null)
                {
                    strUserPhoto = member.UserPhoto;
                    strDateofBirth = member.DateofBirth == null ? string.Empty : ((DateTime)member.DateofBirth).ToString("yyyy/MM/dd");
                    IDCard = member.IDCard ?? string.Empty;
                }
                foreach (Administrator item in listAdmin)
                {
                    if (item.JobPositionId != null && item.JobPosition.IsLeader == true)
                    {
                        strLeader = item.Member.RealName;
                    }
                }
                var data = new
                {
                    AdminName = admin.Member.MemberName,
                    admin.MacAddress,
                    admin.Member.RealName,
                    admin.Member.MobilePhone,
                    admin.Member.Email,
                    UserPhoto = strUserPhoto,
                    DateofBirth = strDateofBirth,
                    DepartmentName = strDepartName,
                    JobPositionName = strJobName,
                    Leader = strLeader,
                    admin.Member.Gender,
                    IDCard = IDCard,
                    EntryTime = admin.EntryTime.ToString("yyyy/MM/dd"),
                };
                oper.ResultType = OperationResultType.Success;
                oper.Data = data;
                return Json(oper);
            }
        }
        #endregion

        #region 修改员工基本信息
        [HttpPost]
        public JsonResult Update(int AdminId, string KeyWord, int UpdateFlag)
        {
            AdminUpdateFlag flag = (AdminUpdateFlag)UpdateFlag;
            OperationResult oper = _administratorContract.Update(AdminId, KeyWord, flag);
            return Json(oper);
        }


        #endregion

        #region 修改头像
        /// <summary>
        /// 上传图片Ios
        /// </summary>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Upload(int AdminId)
        {
            HttpFileCollectionBase files = Request.Files;
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (files.Count == 0)
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                string conPath = ConfigurationHelper.GetAppSetting("SaveMemberPhoto");
                string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
                DateTime now = DateTime.Now;
                Guid gid = Guid.NewGuid();
                string fileName = gid.ToString();
                conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.Hour.ToString() + "/" + now.ToString("yyyyMMddHHmmss") + ".jpg";
                files[0].SaveAs(FileHelper.UrlToPath(conPath));
                Administrator admin = _administratorContract.View(AdminId);
                if (admin == null)
                {
                    oper.Message = "员工不存在";
                }
                else
                {
                    Member member = _memberContract.Members.FirstOrDefault(x => x.Id == admin.MemberId);
                    if (member == null)
                    {
                        oper.Message = "员工不存在";
                    }
                    else
                    {
                        oper = _memberContract.UploadImage(member.Id, conPath);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            oper.Data = strWebUrl + conPath;
                        }
                    }
                }
            }
            return Json(oper);
        }

        /// <summary>
        /// 上传图片前端
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadImg(int AdminId, string Img)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(Img))
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                string[] arrStr = Img.Split(',');
                string strImg = arrStr[1];
                string conPath = ConfigurationHelper.GetAppSetting("SaveMemberPhoto");
                DateTime now = DateTime.Now;
                conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.Hour.ToString() + "/" + now.ToString("yyyyMMddHHmmss") + ".jpg";
                bool res = ImageHelper.SaveBase64Image(strImg, conPath);
                if (res == true)
                {
                    Administrator admin = _administratorContract.View(AdminId);
                    if (admin == null)
                    {
                        oper.Message = "员工不存在";
                    }
                    else
                    {
                        Member member = _memberContract.Members.FirstOrDefault(x => x.Id == admin.MemberId);
                        if (member == null)
                        {
                            oper.Message = "员工不存在";
                        }
                        else
                        {
                            oper = _memberContract.UploadImage(member.Id, conPath);
                            if (oper.ResultType == OperationResultType.Success)
                            {
                                oper.Data = conPath;
                            }
                        }
                    }
                }
                else
                {
                    oper.Message = "保存图片失败";
                }
            }
            return Json(oper);
        }
        #endregion

        #region 修改密码
        public JsonResult UpdatePass(int AdminId, string PassWord, string OldPass)
        {
            OperationResult oper = _administratorContract.UpdatePass(AdminId, OldPass, PassWord);
            return Json(oper);
        }
        #endregion

        #region 申请离职
        public JsonResult ApplyResignation(ResignationDto dto, int AdminId)
        {
            dto.ResignationId = AdminId;
            OperationResult oper = _resignationContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 获取离职
        public JsonResult GetResignation(int AdminId)
        {
            Resignation resignation = _resignationContract.Resignations.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.ResignationId == AdminId && x.ToExamineResult != -6 && x.ToExamineResult != -1);
            string ResignationDate = string.Empty;
            string ResignationReason = string.Empty;
            int PersonnelVerifyType = 0;
            int TechniqueVerifyType = 0;
            int ManagerVerifyType = 0;
            if (resignation != null)
            {
                ResignationDate = resignation.ResignationDate.ToString("yyyy-MM-dd");
                ResignationReason = resignation.ResignationReason;
                //PersonnelVerifyType = resignation.PersonnelVerifyType;
                //TechniqueVerifyType = resignation.TechniqueVerifyType;
                //ManagerVerifyType = resignation.ManagerVerifyType;
            }
            var data = new
            {
                ResignationDate,
                ResignationReason,
                PersonnelVerifyType,
                TechniqueVerifyType,
                ManagerVerifyType,
            };
            OperationResult oper = new OperationResult(OperationResultType.Success, string.Empty, data);
            return Json(oper);
        }
        #endregion

        #region 获取用户积分
        /// <summary>
        /// 获取用户积分
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public JsonResult GetMemberScore(int adminId)
        {
            Administrator admin = _administratorContract.Administrators.FirstOrDefault(a => a.Id == adminId);
            OperationResult oper = new OperationResult(OperationResultType.Error, "用户不存在");
            if (admin == null || admin.Member == null)
            {
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
            oper.ResultType = OperationResultType.Success;
            oper.Data = admin.Member.Score;
            oper.Message = "获取成功";
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取用户积分记录
        /// <summary>
        /// 获取用户积分记录
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="memberOrAdmin">adminId传的为memberId还是adminId(1:adminId,2:memberId)</param>
        /// <returns></returns>
        public JsonResult GetMemberDeposit(int adminId, int pageIndex, int pageSize, int MOrA = 1)
        {
            int memberId = 0;
            OperationResult oper = new OperationResult(OperationResultType.Error, "用户不存在");
            if (MOrA == 1)
            {
                Administrator admin = _administratorContract.Administrators.FirstOrDefault(a => a.Id == adminId);

                if (admin == null || admin.Member == null)
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                memberId = (int)admin.MemberId;
            }
            else
            {

                if (!_memberContract.CheckExists(m => m.Id == adminId))
                {
                    return Json(oper, JsonRequestBehavior.AllowGet);
                }
                memberId = adminId;
            }
            var consumlist = _memberConsumContract.MemberConsumes.Where(c => c.MemberId == memberId && !c.IsDeleted && c.IsEnabled).ToList();
            var depositlist = _memberDepositContract.MemberDeposits.Where(d => d.MemberId == memberId && !d.IsDeleted && d.IsEnabled).ToList();
            var list = (from c in consumlist
                        where c.ScoreConsume != 0
                        select new
                        {
                            Score = "-" + c.ScoreConsume,
                            Notes = c.ConsumeContext.ToString(),
                            CreateTime = c.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss")
                        }).Concat(from d in depositlist
                                  where d.Score != 0
                                  select new
                                  {
                                      Score = "+" + d.Score,
                                      Notes = d.DepositContext.ToString(),
                                      CreateTime = d.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss")
                                  }).ToList();
            oper.Other = list.Count();
            oper.Data = list.OrderByDescending(m => m.CreateTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            oper.ResultType = OperationResultType.Success;
            oper.Message = "获取成功";
            return Json(oper, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public int GetUserSameDayWorkTimeInfo(int adminId)
        {
            var admin = _administratorContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == adminId);
            if (admin == null)
            {
                return 0;
            }
            var msg = string.Empty;
            var dataModel = new WorkTimeDetaile();
            int WhetherToWork = 0;
            if (admin.IsPersonalTime)
            {
                //个人时间 （没有公休假）
                if (admin.WorkTime == null)
                {
                    return 0;
                }
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
                }
                dataModel = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimeId &&
           x.Year == currentYear && x.Month == currentMonth && x.WorkDay == DateTime.Now.Day);
                if (dataModel == null)
                {
                    return 0;
                }
                if (dataModel.WorkTimeType != 2)
                {
                    WhetherToWork = 1;
                }
            }
            else
            {
                //职位时间
                if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                {
                    return 0;
                }
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
                    }
                    if (admin.JobPosition.WorkTime.IsFlexibleWork)
                    {
                        dataModel.AmStartTime = "";
                        dataModel.AmEndTime = "";
                        dataModel.PmStartTime = "";
                        dataModel.PmEndTime = "";
                    }

                }
                else
                {
                    WhetherToWork = 0;
                    if (CheckIsHoliday(1))
                    {
                        WhetherToWork = 1;
                    }
                }
            }
            //var da = new
            //{
            //    msg = msg,
            //    WhetherToWork = WhetherToWork,
            //    IsFlexibleWork = IsFlexibleWork,
            //    AmStartTime = string.IsNullOrEmpty(dataModel.AmStartTime) ? "" : dataModel.AmStartTime,
            //    AmEndTime = string.IsNullOrEmpty(dataModel.AmEndTime) ? "" : dataModel.AmEndTime,
            //    PmStartTime = string.IsNullOrEmpty(dataModel.PmStartTime) ? "" : dataModel.PmStartTime,
            //    PmEndTime = string.IsNullOrEmpty(dataModel.PmEndTime) ? "" : dataModel.PmEndTime
            //};
            return WhetherToWork;
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

        #region 获取用户是否有未读消息
        /// <summary>
        /// 获取用户是否有未读消息
        /// </summary>
        /// <returns></returns>
        private bool ExistUnreadMsg(int adminId)
        {
            int unreadMsgCount = _msgNotificationContract.MsgNotificationReaders.Count(n => !n.IsDeleted && n.IsEnabled && n.AdministratorId == adminId && !n.IsRead);
            unreadMsgCount += _messagerContract.Messagers.Count(m => !m.IsDeleted && m.IsEnabled && m.ReceiverId == adminId && m.Status == 0);
            return unreadMsgCount > 0;
        }
        #endregion
    }

    /// <summary>
    /// 积分记录
    /// </summary>
    public class MemberScoreRecord
    {
        /// <summary>
        /// 积分
        /// </summary>
        public decimal Score { get; set; }

        /// <summary>
        /// 来源
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 获得时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}