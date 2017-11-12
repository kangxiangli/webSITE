using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Filter;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using System.Xml;
using System.IO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class WorkforceManagementController : BaseController
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
        protected readonly IConfigureContract _configureContract;

        public WorkforceManagementController(IAdministratorContract adminContract, IStoreContract storeContract,
            IDepartmentContract departContract,
            IMemberContract memberContract,
            IAdministratorContract administratorContract,
            IWorkTimeContract workTime,
            IHolidayContract holidayContract,
            ILeaveInfoContract leaveInfoContract,
            IOvertimeContract overtimeContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IFieldContract fieldContract,
            IConfigureContract configureContract)
        {
            _adminContract = adminContract;
            _storeContract = storeContract;
            _departContract = departContract;
            _memberContract = memberContract;
            _holidayContract = holidayContract;
            _workTime = workTime;
            _leaveInfoContract = leaveInfoContract;
            _overtimeContract = overtimeContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _fieldContract = fieldContract;
            _configureContract = configureContract;
        }
        [Layout]
        public ActionResult Index()
        {

            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _adminContract, false));
            string SchedulingConfigurationStr = GetXmlNodeByXpath("SchedulingConfiguration");
            string surrDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
            List<string> DataList = new List<string>();
            foreach (var item in SchedulingConfigurationStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (item.Substring(0, 1) == "0")
                {
                    DataList.Add(item.Replace("0", ""));
                }
                else
                {
                    DataList.Add(item);
                }
            }
            ViewBag.IsContains = SchedulingConfigurationStr.Contains(surrDay) ? "1" : "0";
            ViewBag.dataList = string.Join("号、", DataList);
            ViewBag.depList = list;
            return View();
        }

        #region 获取数据列表
        public async Task<ActionResult> List(int? SelDepartmentId)
        {
            try
            {
                var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _adminContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
                GridRequest request = new GridRequest(Request);
                string memberNameOrRealName = Request["MemberName"];
                string mobilePhone = Request["MobilePhone"];
                string IsPersonalTime = Request["IsPersonalTime"];
                Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
                var data = await Task.Run(() =>
                {
                    var count = 0;
                    var query = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled &&
                    listDeps.Contains(x.DepartmentId.Value));
                    if (SelDepartmentId.HasValue)
                    {
                        query = query.Where(w => w.DepartmentId == SelDepartmentId);
                    }
                    if (!string.IsNullOrEmpty(memberNameOrRealName))
                    {
                        query = from a in query
                                join c in _memberContract.Members
                                on a.MemberId equals c.Id into a_cJoin
                                from y in a_cJoin
                                where y.RealName.Contains(memberNameOrRealName) || y.MemberName.Contains(memberNameOrRealName)
                                select a;

                    }
                    if (!string.IsNullOrEmpty(mobilePhone))
                    {
                        query = from a in query
                                join c in _memberContract.Members
                                on a.MemberId equals c.Id into a_cJoin
                                from y in a_cJoin
                                where y.MobilePhone == mobilePhone
                                select a;

                    }
                    List<MemberInfoWork> dataSouce = new List<MemberInfoWork>();
                    if (!string.IsNullOrEmpty(IsPersonalTime))
                    {
                        bool IsPersonal = false;
                        if (IsPersonalTime == "1")
                        {
                            IsPersonal = true;
                        }
                        if (!IsPersonal)
                        {
                            //筛选职位时间
                            var positionQuery = from a in query
                                                join c in _memberContract.Members
                                                on a.MemberId equals c.Id
                                                where !a.IsPersonalTime
                                                select new MemberInfoWork()
                                                {
                                                    Id = a.Id,
                                                    RealName = c.RealName,
                                                    MobilePhone = c.MobilePhone,
                                                    DepartmentName = a.Department.DepartmentName,
                                                    UserPhoto = c.UserPhoto,
                                                    AmEndTime = a.JobPosition == null || a.JobPosition.WorkTime == null ? "" : a.JobPosition.WorkTime.AmEndTime,
                                                    AmStartTime = a.JobPosition == null || a.JobPosition.WorkTime == null ? "" : a.JobPosition.WorkTime.AmStartTime,
                                                    PmEndTime = a.JobPosition == null || a.JobPosition.WorkTime == null ? "" : a.JobPosition.WorkTime.PmEndTime,
                                                    PmStartTime = a.JobPosition == null || a.JobPosition.WorkTime == null ? "" : a.JobPosition.WorkTime.PmStartTime,
                                                    WorkTimeType = a.JobPosition == null || a.JobPosition.WorkTime == null ? "" : a.JobPosition.WorkTime.WorkTimeType.ToString(),
                                                    UseTimeType = a.JobPosition == null || a.JobPosition.WorkTime == null ? false : a.JobPosition.WorkTime.IsPersonalTime,
                                                };
                            var listSource = positionQuery.Distinct().OrderBy(x => x.Id);
                            count = listSource.Count();
                            dataSouce = listSource.Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize)
                                 .ToList();
                        }
                        else
                        {
                            var personalQuery = from a in query
                                                join c in _memberContract.Members
                                                on a.MemberId equals c.Id
                                                where a.IsPersonalTime
                                                select new MemberInfoWork()
                                                {
                                                    Id = a.Id,
                                                    RealName = c.RealName,
                                                    MobilePhone = c.MobilePhone,
                                                    DepartmentName = a.Department.DepartmentName,
                                                    UserPhoto = c.UserPhoto,
                                                    AmEndTime = a.WorkTime == null ? "" : a.WorkTime.AmEndTime,
                                                    AmStartTime = a.WorkTime == null ? "" : a.WorkTime.AmStartTime,
                                                    PmEndTime = a.WorkTime == null ? "" : a.WorkTime.PmEndTime,
                                                    PmStartTime = a.WorkTime == null ? "" : a.WorkTime.PmStartTime,
                                                    WorkTimeType = a.WorkTime == null ? "" : a.WorkTime.WorkTimeType.ToString(),
                                                    UseTimeType = a.WorkTime == null ? false : a.WorkTime.IsPersonalTime,
                                                };
                            var listSource = personalQuery.Distinct().OrderBy(x => x.Id);
                            count = listSource.Count();
                            dataSouce = listSource.Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize)
                                 .ToList();
                        }
                    }
                    else
                    {
                        var positionQuery = from a in query
                                            join c in _memberContract.Members
                                            on a.MemberId equals c.Id
                                            where !a.IsPersonalTime
                                            select new
                                            {
                                                a.Id,
                                                c.MemberName,
                                                c.RealName,
                                                c.MobilePhone,
                                                a.Department.DepartmentName,
                                                a.JobPosition.WorkTime,
                                                c.UserPhoto,

                                            };
                        var personalQuery = from a in query
                                            join c in _memberContract.Members
                                            on a.MemberId equals c.Id
                                            where a.IsPersonalTime
                                            select new
                                            {
                                                a.Id,
                                                c.MemberName,
                                                c.RealName,
                                                c.MobilePhone,
                                                a.Department.DepartmentName,
                                                a.WorkTime,
                                                c.UserPhoto
                                            };
                        var listSource = positionQuery.Union(personalQuery).Distinct().OrderBy(x => x.Id).Select(a => new MemberInfoWork()
                        {
                            Id = a.Id,
                            RealName = a.RealName,
                            MobilePhone = a.MobilePhone,
                            DepartmentName = a.DepartmentName,
                            UserPhoto = a.UserPhoto,
                            AmEndTime = a.WorkTime == null ? "" : a.WorkTime.AmEndTime,
                            AmStartTime = a.WorkTime == null ? "" : a.WorkTime.AmStartTime,
                            PmEndTime = a.WorkTime == null ? "" : a.WorkTime.PmEndTime,
                            PmStartTime = a.WorkTime == null ? "" : a.WorkTime.PmStartTime,
                            WorkTimeType = a.WorkTime == null ? "" : a.WorkTime.WorkTimeType.ToString(),
                            UseTimeType = a.WorkTime == null ? false : a.WorkTime.IsPersonalTime
                        });
                        count = listSource.Count();
                        dataSouce = listSource.Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize)
                             .ToList();
                    }
                    List<object> listTrue = new List<object>();
                    foreach (var item in dataSouce)
                    {
                        int WorkWeekCount = GetWorkWeekCount(item.Id);
                        int _workDetaileCount = WorkDetaileCount(item.Id);
                        var da = new
                        {
                            item.Id,
                            item.RealName,
                            item.MobilePhone,
                            item.DepartmentName,
                            item.WorkTimeType,
                            item.UseTimeType,
                            item.UserPhoto,
                            AmEndTime = Convert.ToDateTime(item.AmEndTime).ToShortTimeString(),
                            AmStartTime = Convert.ToDateTime(item.AmStartTime).ToShortTimeString(),
                            PmEndTime = Convert.ToDateTime(item.PmEndTime).ToShortTimeString(),
                            PmStartTime = Convert.ToDateTime(item.PmStartTime).ToShortTimeString(),
                            WorkWeekCount = WorkWeekCount,
                            WorkDetaileCount = _workDetaileCount
                        };
                        listTrue.Add(da);
                    }
                    return new GridData<object>(listTrue, count, request.RequestInfo);
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public int WorkDetaileCount(int Id)
        {
            var admin = _adminContract.Administrators.FirstOrDefault(x => x.Id == Id && !x.IsDeleted && x.IsEnabled);
            if (admin == null)
            {
                return 0;
            }
            if (!admin.IsPersonalTime)
            {
                return 0;
            }
            if (admin.WorkTime == null)
            {
                return 0;
            }
            return _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id).Count();
        }


        public ActionResult SchedulingConfigurationIndex()
        {
            string SchedulingConfigurationStr = GetXmlNodeByXpath("SchedulingConfiguration");
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(SchedulingConfigurationStr))
            {
                foreach (var item in SchedulingConfigurationStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    list.Add(item.PadLeft(2, '0'));
                }
            }
            ViewBag.SchedulingConfiguration = list;
            ViewBag.AdvanceDay = GetXmlNodeByXpath("AdvanceDay");
            return PartialView();
        }
        public ActionResult SchedulingConfiguration(string SchedulingConfigurationStr, string AdvanceDay)
        {
            UpdateNode("SchedulingConfiguration", SchedulingConfigurationStr);
            //AdvanceDay
            UpdateNode("AdvanceDay", AdvanceDay);
            string surrDay = DateTime.Now.Day.ToString().PadLeft(2, '0');
            List<string> DataList = new List<string>();
            foreach (var item in SchedulingConfigurationStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (item.Substring(0, 1) == "0")
                {
                    DataList.Add(item.Replace("0", ""));
                }
                else
                {
                    DataList.Add(item);
                }
            }
            string IsContains = SchedulingConfigurationStr.Contains(surrDay) ? "1" : "0";
            string dataList = string.Join("号、", DataList);
            var da = new { IsContains = IsContains, dataList = dataList };
            return Json(da);
        }
        public int GetWorkWeekCount(int? Id)
        {
            int count = 0;
            Id = Id == null ? 0 : Id;
            var admin = _adminContract.Administrators.Where(x => x.Id == Id).FirstOrDefault();
            bool position = false;
            if (admin.WorkTime == null || (admin.WorkTime != null && !admin.WorkTime.IsPersonalTime))
            {
                position = true;
            }
            string _workWeek = string.Empty;
            if (position)
            {
                //使用职位时间
                _workWeek = admin.JobPosition != null && admin.JobPosition.WorkTime != null ? admin.JobPosition.WorkTime.WorkWeek : string.Empty;
            }
            else
            {
                //使用自定义时间
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
                var WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
             && x.Month == currentMonth).ToList();
                _workWeek = string.Join(",", WtdArry.Where(x => x.WorkTimeType != 2).OrderBy(x => x.WorkDay)
                    .Select(x => x.WorkDay.ToString().PadLeft(2, '0')).ToArray());
            }
            count = string.IsNullOrEmpty(_workWeek) ? 0 : _workWeek.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return count;
        }

        public ActionResult GetWorkWeek(int Id)
        {
            ViewBag.WorkTimeId = Id;
            return PartialView();
        }

        public ActionResult GetWorkWeekInfo(int Id)
        {
            var admin = _adminContract.Administrators.Where(x => x.Id == Id).FirstOrDefault();
            bool position = false;
            if (admin.WorkTime == null || (admin.WorkTime != null && !admin.WorkTime.IsPersonalTime))
            {
                position = true;
            }
            string _workWeek = string.Empty;
            List<object> list = new List<object>();
            if (position)
            {
                //使用职位时间
                _workWeek = admin.JobPosition == null || admin.JobPosition.WorkTime == null ? string.Empty : admin.JobPosition.WorkTime.WorkWeek;
                if (string.IsNullOrEmpty(_workWeek))
                {
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in _workWeek.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string weekday = "";
                    switch (item)
                    {
                        case "1":
                            weekday = "周一";
                            break;
                        case "2":
                            weekday = "周二";
                            break;
                        case "3":
                            weekday = "周三";
                            break;
                        case "4":
                            weekday = "周四";
                            break;
                        case "5":
                            weekday = "周五";
                            break;
                        case "6":
                            weekday = "周六";
                            break;
                        case "0":
                            weekday = "周日";
                            break;
                    }
                    if (weekday == "")
                    {
                        continue;
                    }
                    var da = new
                    {
                        weekday = weekday,
                        AmStartTime = admin.JobPosition.WorkTime.AmStartTime,
                        AmEndTime = admin.JobPosition.WorkTime.AmEndTime,
                        PmEndTime = admin.JobPosition.WorkTime.PmEndTime,
                        PmStartTime = admin.JobPosition.WorkTime.PmStartTime
                    };
                    list.Add(da);
                }
            }
            else
            {
                //使用自定义时间
                _workWeek = admin.WorkTime == null ? string.Empty : admin.WorkTime.WorkWeek;
                if (string.IsNullOrEmpty(_workWeek))
                {
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                foreach (var item in _workWeek.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (item == "")
                    {
                        continue;
                    }
                    int workDay = 0;
                    var workdayStr = item.Substring(0, 1) == "0" ? item.Replace("0", "") : item;
                    int.TryParse(workdayStr, out workDay);
                    var model = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkDay == workDay).FirstOrDefault();

                    if (model == null)
                    {
                        continue;
                    }
                    var da = new
                    {
                        weekday = workdayStr,
                        AmStartTime = model.AmStartTime,
                        AmEndTime = model.AmEndTime,
                        PmEndTime = model.PmEndTime,
                        PmStartTime = model.PmStartTime
                    };
                    list.Add(da);
                }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewUpdate(int Id)
        {
            Administrator admi = _adminContract.Administrators.Where(c => c.Id == Id).FirstOrDefault();
            ViewBag.adminId = Id;
            int workTimrId = 0;
            WorkTime workTime = new WorkTime()
            {
                AmStartTime = "0:0",
                AmEndTime = "0:0",
                PmStartTime = "0:0",
                PmEndTime = "0:0",
                IsEnabled = false,
            };
            var WtdArry = new List<WorkTimeDetaile>();
            if (admi.WorkTime != null)
            {
                workTimrId = admi.WorkTime.Id;
                workTime = admi.WorkTime;

                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;
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
                WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
            && x.Month == currentMonth).ToList();
                workTime.WorkWeek = string.Join(",", WtdArry.Where(x => x.WorkTimeType != 2).OrderBy(x => x.WorkDay)
                    .Select(x => x.WorkDay.ToString().PadLeft(2, '0')).ToArray());
            }
            workTime.AdminId = Id;
            ViewBag.WtdArry = WtdArry;
            List<DateTime> dateList = new List<DateTime>();
            string starStr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month + "-1";
            DateTime timeStart = Convert.ToDateTime(starStr);
            DateTime startDate = Convert.ToDateTime(starStr);
            dateList.Add(startDate);
            for (int i = 2; i <= DateTime.Now.Day; i++)
            {
                startDate = startDate.AddDays(1);
                dateList.Add(startDate);
            }
            List<TipsInfo> tipeList = new List<TipsInfo>();
            var overT = _overtimeContract.Overtimes.Where(x => !x.IsDeleted && x.IsEnabled && x.VerifyType == 1 && x.AdminId == Id
              && ((x.StartTime.Year == DateTime.Now.Year && x.StartTime.Month == DateTime.Now.Month) ||
              (x.EndTime.Year == DateTime.Now.Year && x.EndTime.Month == DateTime.Now.Month))).Select(x => new TipsInfo()
              {
                  Id = x.Id,
                  StartTime = x.StartTime,
                  EndTime = x.EndTime,
                  TypeDesc = "加班"
              });
            tipeList.AddRange(overT);
            var leaveT = _leaveInfoContract.LeaveInfos.Where(x => !x.IsDeleted && x.IsEnabled && x.VerifyType == 1 && x.AdminId == Id
                && ((x.StartTime.Year == DateTime.Now.Year && x.StartTime.Month == DateTime.Now.Month) ||
                (x.EndTime.Year == DateTime.Now.Year && x.EndTime.Month == DateTime.Now.Month))).Select(x => new TipsInfo()
                {
                    Id = x.Id,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    TypeDesc = "请假"
                });
            tipeList.AddRange(leaveT);
            var fileT = _fieldContract.Fields.Where(x => !x.IsDeleted && x.IsEnabled && x.VerifyType == 1 && x.AdminId == Id
                && ((x.StartTime.Year == DateTime.Now.Year && x.StartTime.Month == DateTime.Now.Month) ||
                (x.EndTime.Year == DateTime.Now.Year && x.EndTime.Month == DateTime.Now.Month))).Select(x => new TipsInfo()
                {
                    Id = x.Id,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    TypeDesc = "外勤"
                });
            tipeList.AddRange(fileT);
            Dictionary<string, List<TipsInfo>> dicList = new Dictionary<string, List<TipsInfo>>();
            foreach (var item in tipeList)
            {
                if (item.StartTime == null || item.EndTime == null)
                {
                    continue;
                }
                foreach (var dateItem in dateList)
                {
                    if (DateTime.Compare(dateItem, DateTime.Parse(item.StartTime.ToShortDateString())) < 0 ||
                        DateTime.Compare(DateTime.Parse(item.EndTime.ToShortDateString()), dateItem) < 0)
                    {
                        continue;
                    }
                    string currentdayStr = dateItem.Day.ToString().PadLeft(2, '0');
                    if (dicList.Keys.Contains(currentdayStr))
                    {
                        var itemList = dicList[currentdayStr];
                        itemList.Add(item);
                        dicList[currentdayStr] = itemList;
                    }
                    else
                    {
                        var item_list = new List<TipsInfo>();
                        item_list.Add(item);
                        dicList.Add(currentdayStr, item_list);
                    }
                }
            }
            var dataSource = new List<object>();
            foreach (var key_item in dicList.Keys)
            {
                var da = new
                {
                    day = key_item,
                    data = dicList[key_item]
                };
                dataSource.Add(da);
            }
            ViewBag.dicList = Newtonsoft.Json.JsonConvert.SerializeObject(dataSource);
            return PartialView(workTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="currentOrNext">当月排班还是下月排班（1：当月；2：下月）</param>
        /// <returns></returns>
        public ActionResult Update(int Id, int currentOrNext = 2)
        {

            Administrator admi = _adminContract.Administrators.FirstOrDefault(c => c.Id == Id);
            ViewBag.adminId = Id;
            int workTimrId = 0;
            string isChange = "0";

            WorkTime workTime = new WorkTime()
            {
                AmStartTime = "0:0",
                AmEndTime = "0:0",
                PmStartTime = "0:0",
                PmEndTime = "0:0",
                IsEnabled = false,
            };
            var WtdArry = new List<WorkTimeDetaile>();

            if (admi != null)
            {
                if ((admi.whetherChange != null && admi.whetherDateTime != null && (DateTime.Now - admi.whetherDateTime.Value).TotalDays > 6) || (admi.whetherChange == null || admi.whetherDateTime == null))
                {
                    isChange = "1";
                }

                if (admi.WorkTime != null)
                {
                    workTimrId = admi.WorkTime.Id;
                    workTime = admi.WorkTime;

                    int currentYear = DateTime.Now.Year;
                    int currentMonth = DateTime.Now.Month;
                    var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
                    if (monthList.Count == 2)
                    {
                        int month = monthList.Min();
                        int minYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == month).Year;
                        int maxMonth = monthList.Max();
                        int maxYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == maxMonth).Year;
                        //if (monthList.Contains(currentMonth))
                        //{
                        if ((month == currentMonth && minYear == currentYear) || (maxMonth == currentMonth && maxYear == currentYear))
                        {
                            if (currentOrNext == 2)
                            {//如果是下月排班，则按照以下规则
                                //if (monthList.Contains(12) && monthList.Contains(1))
                                //{
                                //    month = 12;
                                //    currentYear = currentYear - 1;
                                //}
                                if (minYear > maxYear)
                                {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                                    if (maxMonth == currentMonth)
                                    {//若当月为12月，则下月为一月份
                                        currentMonth = month;
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
                                    if (maxMonth > currentMonth)
                                    {
                                        month = maxMonth;
                                    }
                                }
                            }
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
                                currentYear = minYear;
                            }
                        }
                        WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
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
                        WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
                        && x.Month == currentMonth).ToList();
                    }
                }
            }
            workTime.AdminId = Id;
            ViewBag.WtdArry = WtdArry;
            ViewBag.isChange = isChange;
            ViewBag.CurrentOrNext = currentOrNext;
            return PartialView(workTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WorkWeekArry"></param>
        /// <param name="worktime"></param>
        /// <param name="wtdArry"></param>
        /// <param name="currentOrNext">当月排班还是下月排班（1：当月；2：下月）</param>
        /// <returns></returns>
        public ActionResult UpdateWorkTime(string WorkWeekArry, WorkTime worktime, WorkTimeDetaile[] wtdArry, int currentOrNext = 2)
        {
            if (worktime == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "当前没有排班时间表"));
            }
            OperationResult result = new OperationResult(OperationResultType.Error);
            int? Id = worktime.AdminId;
            worktime.WorkWeek = WorkWeekArry;
            string retweet = Request["retweet"];
            Administrator admin = _adminContract.Administrators.Where(c => c.Id == Id).FirstOrDefault();
            int countDay = 0;
            int currentYear = DateTime.Now.Year;
            bool whetherChange = false;

            if (admin.IsPersonalTime != worktime.IsPersonalTime || admin.whetherChange == null)
            {
                whetherChange = true;
                admin.whetherChange = whetherChange;
                admin.whetherDateTime = DateTime.Now;
            }
            int currentMonth = DateTime.Now.Month;

            if (currentOrNext == 2)
            {//如果是下月排班，则按照以下规则
                if (currentMonth == 12)
                {
                    currentYear = currentYear + 1;
                    currentMonth = 1;
                }
                else
                {
                    currentMonth = currentMonth + 1;
                }
            }
            if (worktime.IsPersonalTime)
            {
                admin.IsPersonalTime = true;
                if (admin.WorkTime != null)
                {
                    admin.WorkTime.WorkWeek = WorkWeekArry;
                    admin.WorkTime.IsPersonalTime = worktime.IsPersonalTime;
                    admin.WorkTime.IsFlexibleWork = worktime.IsFlexibleWork;
                    admin.WorkTime.WorkTimeName = worktime.WorkTimeName;
                    admin.WorkTime.IsVacations = worktime.IsVacations;
                    admin.WorkTime.WorkTimeType = worktime.WorkTimeType;
                    admin.WorkTime.WorkHour = worktime.WorkHour;
                    admin.WorkTime.Summary = worktime.Summary;

                    countDay = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id
                    && x.Year == currentYear && x.Month == currentMonth).Count();
                }
                else
                {
                    admin.WorkTime = worktime;
                    admin.WorkTime.AmEndTime = string.IsNullOrEmpty(worktime.AmEndTime) ? "00:00" : Convert.ToDateTime(worktime.AmEndTime).ToShortTimeString();
                    admin.WorkTime.AmStartTime = string.IsNullOrEmpty(worktime.AmStartTime) ? "00:00" : Convert.ToDateTime(worktime.AmStartTime).ToShortTimeString();
                    admin.WorkTime.PmEndTime = string.IsNullOrEmpty(worktime.PmEndTime) ? "00:00" : Convert.ToDateTime(worktime.PmEndTime).ToShortTimeString();
                    admin.WorkTime.PmStartTime = string.IsNullOrEmpty(worktime.PmStartTime) ? "00:00" : Convert.ToDateTime(worktime.PmStartTime).ToShortTimeString();
                    admin.WorkTime.WorkWeek = WorkWeekArry;
                }
                int totalNumber = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id).Count();
                if ((countDay == 0 || countDay == 31) && totalNumber == 62)
                {
                    var dtoArry = new List<WorkTimeDetaileDto>();
                    foreach (var item in wtdArry)
                    {
                        WorkTimeDetaileDto dto = new WorkTimeDetaileDto();
                        dto.Id = item.Id;
                        dto.WorkTimeId = admin.WorkTime.Id;
                        dto.IsFlexibleWork = item.IsFlexibleWork;
                        dto.WorkDay = item.WorkDay;
                        dto.WorkHour = item.WorkHour;
                        dto.WorkTimeType = item.WorkTimeType;
                        dto.Year = currentYear;
                        dto.Month = currentMonth;
                        if (item.IsFlexibleWork || dto.WorkTimeType == 2)
                        {
                            dto.PmStartTime = "00:00";
                            dto.PmEndTime = "00:00";
                            dto.AmStartTime = "00:00";
                            dto.AmEndTime = "00:00";
                        }
                        else
                        {
                            dto.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                            dto.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                            dto.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                            dto.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                        }
                        dtoArry.Add(dto);
                    }
                    result = _workTimeDetaileContract.Update(dtoArry.ToArray());
                    if (result.ResultType != OperationResultType.Success)
                    {
                        return Json(result);
                    }
                }
                else
                {
                    var oper = _workTimeDetaileContract.TrueRemove(currentYear, currentMonth, admin.WorkTime.Id);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        return Json(oper);
                    }
                    result = _adminContract.Update(admin);
                    if (result.ResultType != OperationResultType.Success)
                    {
                        return Json(oper);
                    }
                    admin = _adminContract.Administrators.Where(c => c.Id == Id).FirstOrDefault();
                    List<WorkTimeDetaile> listArry = new List<WorkTimeDetaile>();
                    if (admin.WorkTimeId == 0)
                    {
                        return Json(oper);
                    }
                    foreach (var item in wtdArry)
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
                    int monthS = currentMonth;
                    int yearS = currentYear;
                    if (currentOrNext == 2)
                    {
                        if (currentMonth == 1)
                        {
                            monthS = 12;
                            yearS = currentYear - 1;
                        }
                        else
                        {
                            monthS = currentMonth - 1;
                        }
                    }
                    else
                    {
                        if (currentMonth == 12)
                        {
                            monthS = 1;
                            yearS = currentYear + 1;
                        }
                        else
                        {
                            monthS = currentMonth + 1;
                        }
                    }
                    int currentCount = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id
          && x.Year == yearS && x.Month == monthS).Count();
                    if (currentCount == 0)
                    {
                        foreach (var item in wtdArry)
                        {
                            WorkTimeDetaile wd = new WorkTimeDetaile();
                            wd.Id = 0;
                            wd.WorkTimeId = admin.WorkTime.Id;
                            wd.IsFlexibleWork = item.IsFlexibleWork;
                            wd.WorkDay = item.WorkDay;
                            wd.WorkHour = item.WorkHour;
                            wd.WorkTimeType = item.WorkTimeType;
                            wd.Year = yearS;
                            wd.Month = monthS;
                            wd.AmEndTime = string.IsNullOrEmpty(item.AmEndTime) ? "00:00" : Convert.ToDateTime(item.AmEndTime).ToShortTimeString();
                            wd.AmStartTime = string.IsNullOrEmpty(item.AmStartTime) ? "00:00" : Convert.ToDateTime(item.AmStartTime).ToShortTimeString();
                            wd.PmEndTime = string.IsNullOrEmpty(item.PmEndTime) ? "00:00" : Convert.ToDateTime(item.PmEndTime).ToShortTimeString();
                            wd.PmStartTime = string.IsNullOrEmpty(item.PmStartTime) ? "00:00" : Convert.ToDateTime(item.PmStartTime).ToShortTimeString();
                            listArry.Add(wd);
                        }
                    }
                    result = _workTimeDetaileContract.Insert(listArry.ToArray());
                    if (result.ResultType != OperationResultType.Success)
                    {
                        return Json(oper);
                    }

                }
            }
            else
            {
                admin.IsPersonalTime = false;
                if (admin.WorkTime != null)
                {
                    admin.WorkTime.IsPersonalTime = false;
                }
                admin.IsPersonalTime = worktime.IsPersonalTime;
                result = _adminContract.Update(admin);
            }
            return Json(result);
        }


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

        public ActionResult DutyInquiryIndex()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _adminContract, false));
            ViewBag.depList = list;
            return PartialView();
        }

        public ActionResult DutyInquiryList(int? SelDepartmentId)
        {
            try
            {
                GridRequest request = new GridRequest(Request);

                Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
                var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _adminContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
                string memberName = Request["memberName"];
                var count = 0;
                var query = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.MemberId != null);
                //筛选职位时间
                var positionQuery = from a in query
                                    join c in _memberContract.Members
                                    on a.MemberId equals c.Id
                                    where !a.IsPersonalTime
                                    select new
                                    {
                                        a.Id,
                                        c.MemberName,
                                        DepartmentId = a.DepartmentId == null ? 0 : a.DepartmentId.Value,
                                        a.MemberId,
                                        c.RealName,
                                        c.MobilePhone,
                                        a.Department.DepartmentName,
                                        a.JobPosition.WorkTime,
                                        a.IsPersonalTime
                                    };
                var personalQuery = from a in query
                                    join c in _memberContract.Members
                                    on a.MemberId equals c.Id
                                    where a.IsPersonalTime
                                    select new
                                    {
                                        a.Id,
                                        c.MemberName,
                                        DepartmentId = a.DepartmentId == null ? 0 : a.DepartmentId.Value,
                                        a.MemberId,
                                        c.RealName,
                                        c.MobilePhone,
                                        a.Department.DepartmentName,
                                        a.WorkTime,
                                        a.IsPersonalTime
                                    };
                var list = positionQuery.Union(personalQuery).ToList();
                //筛选公休假
                if (IsHoliday() > 0)
                {
                    //在公休假内 筛选掉享受公休假的
                    list = list.Where(x => x.WorkTime != null).ToList();
                    list = list.Where(x => !x.WorkTime.IsVacations).ToList();
                }
                //筛选掉请假
                var leaveList = IsLeaveInfo();
                list = list.Where(x => !leaveList.Contains(x.Id)).ToList();
                List<MemberInfoWork> listSouce = new List<MemberInfoWork>();
                string week = DateTime.Now.DayOfWeek.ToString("d");
                int currentDay = DateTime.Now.Day;
                foreach (var item in list)
                {
                    if (item.WorkTime == null)
                    {
                        continue;
                    }
                    if (!item.IsPersonalTime)
                    {
                        if (DateTime.Now > Convert.ToDateTime(item.WorkTime.AmStartTime)
                            && DateTime.Now < Convert.ToDateTime(item.WorkTime.PmEndTime)
                            && item.WorkTime.WorkWeek.Contains(week))
                        {
                            MemberInfoWork miw = new Controllers.MemberInfoWork();
                            miw.Id = item.Id;
                            miw.DepartmentId = item.DepartmentId;
                            miw.MemberId = item.MemberId;
                            miw.DepartmentName = item.DepartmentName;
                            miw.RealName = item.RealName;
                            miw.MobilePhone = item.MobilePhone;
                            listSouce.Add(miw);
                        }
                    }
                    else
                    {
                        var workdeatile = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == item.WorkTime.Id && x.WorkDay == currentDay)
                            .FirstOrDefault();
                        if (workdeatile != null)
                        {
                            if (DateTime.Now > Convert.ToDateTime(workdeatile.AmStartTime)
&& DateTime.Now < Convert.ToDateTime(workdeatile.PmEndTime) && workdeatile.WorkTimeType != 2)
                            {
                                MemberInfoWork miw = new Controllers.MemberInfoWork();
                                miw.Id = item.Id;
                                miw.DepartmentId = item.DepartmentId;
                                miw.MemberId = item.MemberId;
                                miw.DepartmentName = item.DepartmentName;
                                miw.RealName = item.RealName;
                                miw.MobilePhone = item.MobilePhone;
                                listSouce.Add(miw);
                            }
                        }
                    }
                }
                //添加享有公休假 需补班的人员
                List<int> addList = new List<int>();
                addList = addList.Union(OvertimeList()).Union(GetMaeUpClass()).Distinct().ToList();
                var addModel = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled &&
                addList.Contains(x.Id)).Select(x => new MemberInfoWork()
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId.Value,
                    MemberId = x.MemberId,
                    DepartmentName = x.Department.DepartmentName,
                    RealName = x.Member.RealName,
                    MobilePhone = x.Member.MobilePhone
                });
                listSouce = listSouce.Union(addModel).ToList();
                if (SelDepartmentId != null)
                {
                    listSouce = listSouce.Where(x => x.DepartmentId == SelDepartmentId).ToList();
                }
                else
                {
                    listSouce = listSouce.Where(x => listDeps.Contains(x.DepartmentId)).ToList();
                }
                if (!string.IsNullOrEmpty(memberName))
                {
                    listSouce = listSouce.Where(x => x.RealName.Contains(memberName)).ToList();
                }

                List<MemberInfoWork> lastData = new List<MemberInfoWork>();
                List<int> adminList = new List<int>();
                foreach (var item in listSouce)
                {
                    if (!adminList.Contains(item.Id))
                    {
                        lastData.Add(item);
                        adminList.Add(item.Id);
                    }
                }
                count = lastData.Count();
                var dataSouce = lastData.OrderBy(x => x.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize)
                     .ToList();
                var data = new GridData<object>(dataSouce, count, request.RequestInfo);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public int IsHoliday()
        {
            DateTime currdate = DateTime.Now.Date;
            int count = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled
            && x.StartTime <= currdate && x.EndTime >= currdate).Count();
            return count;
        }
        public List<int> IsLeaveInfo()
        {
            DateTime currdate = DateTime.Now;
            var leavelist = _leaveInfoContract.LeaveInfos.Where(x => !x.IsDeleted && x.IsEnabled
            && x.StartTime <= currdate && x.EndTime >= currdate && x.VerifyType == 1).Select(x => x.AdminId).ToList();
            return leavelist;
        }

        public bool IsMakeUpClass()
        {
            string currdate = DateTime.Now.Date.ToString("yyyy/MM/dd");
            int count = _holidayContract.Holidays.Where(x => !x.IsDeleted && x.IsEnabled
            && x.WorkDates.Contains(currdate)).Count();
            return count > 0 ? true : false;
        }
        public List<int> OvertimeList()
        {
            DateTime currdate = DateTime.Now;
            var leavelist = _overtimeContract.Overtimes.Where(x => !x.IsDeleted && x.IsEnabled
            && x.StartTime <= currdate && x.EndTime >= currdate && x.VerifyType == 1).Select(x => x.AdminId).ToList();
            return leavelist;
        }

        //添加享有公休假 需补班的人员
        public List<int> GetMaeUpClass()
        {
            List<int> list = new List<int>();
            if (IsMakeUpClass())
            {
                //职位时间享有公休假 
                var postion = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && !x.IsPersonalTime
                  && x.JobPosition != null && x.JobPosition.WorkTime != null && x.JobPosition.WorkTime.IsVacations).Select(x => x.Id).ToList();

                //使用个人时间享受公休假
                var personal = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && !x.IsPersonalTime
                  && x.WorkTime != null && x.WorkTime.IsVacations).Select(x => x.Id).ToList();

                list = list.Union(postion).Union(personal).Distinct().ToList();
            }
            return list;
        }

        public string GetXmlNodeByXpath(string xmlFileName)
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
                return "";
                //throw ex; //这里可以定义你自己的异常处理
            }
        }

        ///<summary>
        /// 修改节点
        ///</summary>
        public void UpdateNode(string xmlFileName, string innerText)
        {
            //string xpath = Server.MapPath("/Content/Config/SchedulingConfiguration/Scheduling.xml");
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(xpath); //加载xml文件
            //XmlNode xmlNode = xmlDoc.SelectSingleNode("User").SelectSingleNode(xmlFileName);
            //XmlElement xe = (XmlElement)xmlNode;
            //xe.InnerText = innerText;
            //xmlDoc.Save(xpath);//保存。 
            _configureContract.SetConfigure("SchedulingConfiguration", "Scheduling", xmlFileName, innerText);
        }

        public ActionResult Retweet(int Id)
        {
            Administrator admi = _adminContract.Administrators.FirstOrDefault(c => c.Id == Id);
            if (admi == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"));
            }
            if (admi.WorkTime == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "当前没有排班时间表"));
            }
            int workTimeId = admi.WorkTime.Id;
            var oper = _workTimeDetaileContract.TrueRemoveByWorkTimeId(workTimeId);
            if (oper.ResultType != OperationResultType.Success)
            {
                return Json(oper);
            }
            var model = _workTime.WorkTimes.Where(x => x.Id == workTimeId).Select(x => new WorkTimeDto()
            {
                Id = x.Id,
                WorkTimeName = "",
                WorkTimeType = x.WorkTimeType,
                TimeType = x.TimeType,
                IsPersonalTime = x.IsPersonalTime,
                IsVacations = x.IsVacations,
                IsFlexibleWork = x.IsFlexibleWork,
                AmStartTime = x.AmStartTime,
                AmEndTime = x.AmEndTime,
                PmStartTime = x.PmStartTime,
                PmEndTime = x.PmEndTime,
                WorkWeek = "",
                WorkHour = x.WorkHour,
                Summary = x.Summary,
                IsEnabled = x.IsEnabled,
                AdminId = admi.Id
            }).FirstOrDefault();
            oper = _workTime.Update(model);
            return Json(oper);
        }

    }

    public class MemberInfoWork
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string RealName { get; set; }
        public string MobilePhone { get; set; }
        public string WorkTimeType { get; set; }
        public string UserPhoto { get; set; }
        public bool UseTimeType { get; set; }
        public int WorkWeekCount { get; set; }
        public string PmStartTime { get; set; }
        public string PmEndTime { get; set; }
        public string AmStartTime { get; set; }
        public string AmEndTime { get; set; }
        public int DepartmentId { get; set; }
        public int? MemberId { get; set; }
    }

    public class TipsInfo
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TypeDesc { get; set; }
    }
}