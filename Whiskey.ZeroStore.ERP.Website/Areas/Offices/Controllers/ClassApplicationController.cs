using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Threading.Tasks;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Web.Mvc;
using System.Xml;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class ClassApplicationController : BaseController
    {
        private readonly IClassApplicationContract _classApplicationContract;
        private readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        private readonly IAdministratorContract _administratorContract;
        private readonly IDepartmentContract _departmentContract;
        private readonly IConfigureContract _configureContract;
        private int AdvanceDay = 0;
        public ClassApplicationController(IClassApplicationContract classApplicationContract,
            IWorkTimeDetaileContract workTimeDetaileContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IConfigureContract configureContract)
        {
            _classApplicationContract = classApplicationContract;
            _administratorContract = administratorContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _departmentContract = departmentContract;
            _configureContract = configureContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string SuccessionName = Request["SuccessionName"];
            Expression<Func<ClassApplication, bool>> predicate = FilterHelper.GetExpression<ClassApplication>(request.FilterGroup);
            int PageIndex = request.PageCondition.PageIndex + 1;
            int PageSize = request.PageCondition.PageSize;
            var listData = _classApplicationContract.ClassApplications.Where(predicate).Where(x => x.AdminId == AuthorityHelper.OperatorId.Value);
            var data = await Task.Run(() =>
            {
                var count = 0;
                listData = listData.Where(x => x.Succession.Member.RealName.Contains(SuccessionName));
                count = listData.Count();
                var list = listData.OrderByDescending(c => c.CreatedTime).Skip(request.PageCondition.PageIndex * PageSize).Take(PageIndex * PageSize).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    x.CreatedTime,
                    x.Day,
                    x.OffDay,
                    x.SuccessionDep.DepartmentName,
                    x.UpdatedTime,
                    x.ToExamineResult,
                    x.IsDeleted,
                    SuccessionName = x.Succession.Member.RealName
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            AdvanceDay = Convert.ToInt32(GetXmlNodeByXpath("AdvanceDay"));
            var admin = _administratorContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == AuthorityHelper.OperatorId.Value);
            List<SelectListItem> workDayList = new List<SelectListItem>();
            List<SelectListItem> offDayList = new List<SelectListItem>();
            workDayList.Add(new SelectListItem() { Value = "", Text = "请选择" });
            offDayList.Add(new SelectListItem() { Value = "", Text = "请选择" });
            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            if (admin.IsPersonalTime && admin.WorkTime != null)
            {
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
                    //if (monthList.Contains(currentMonth))
                    //{
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
                    //if ((minMonth == currentMonth && minYear == currentYear) || (maxMonth == currentMonth && maxYear == currentYear))
                    //{
                    //    //if (monthList.Contains(12) && monthList.Contains(1))
                    //    //{
                    //    //}
                    //    //else
                    //    //{
                    //    //if (currentMonth == maxMonth)
                    //    //{
                    //    //    currentMonth = maxMonth;
                    //    //}
                    //    //else
                    //    //{
                    //    //    currentMonth = minMonth;
                    //    //}
                    //    //}
                    //}
                    //else
                    //{
                    //    currentMonth = minMonth;
                    //}
                }
                var WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
&& x.Month == currentMonth && x.WorkTimeType != 2).ToList();
                currentDay = DateTime.Now.Day + AdvanceDay;
                if (WtdArry != null && WtdArry.Count() > 0)
                {
                    var dayArry = WtdArry.Select(w => w.WorkDay);
                    foreach (var item in dayArry)
                    {
                        if (Convert.ToInt32(item) > currentDay && Convert.ToInt32(item) <= days)
                        {
                            workDayList.Add(new SelectListItem() { Value = item.ToString(), Text = item + "号" });
                        }

                    }
                    var offDay = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkTimeType == 2 && x.Year == currentYear
&& x.Month == currentMonth)
                        .Select(x => x.WorkDay);
                    foreach (var item in offDay)
                    {
                        if (item > currentDay && item <= days)
                        {
                            offDayList.Add(new SelectListItem() { Value = item.ToString(), Text = item + "号" });
                        }
                    }
                }
            }
            var DepList = new List<SelectListItem>();
            ClassApplication ca = new ClassApplication();
            DepList.AddRange(_departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled).Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.DepartmentName
            }));
            ViewBag.workDayList = workDayList;
            ViewBag.offDayList = offDayList;
            ViewBag.DepList = DepList;
            ViewBag.RealName = AuthorityHelper.RealName;
            ViewBag.adminId = AuthorityHelper.OperatorId ?? 0;
            return PartialView();
        }
        [HttpPost]
        public JsonResult Create(ClassApplication dto)
        {
            dto.OperatorId = AuthorityHelper.OperatorId.Value;
            OperationResult oper = _classApplicationContract.Insert(dto);
            return Json(oper);
        }

        public ActionResult Update(int Id)
        {
            AdvanceDay = Convert.ToInt32(GetXmlNodeByXpath("AdvanceDay"));
            var admin = _administratorContract.Administrators.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.Id == AuthorityHelper.OperatorId.Value);
            List<SelectListItem> workDayList = new List<SelectListItem>();
            List<SelectListItem> offDayList = new List<SelectListItem>();
            workDayList.Add(new SelectListItem() { Value = "", Text = "请选择" });
            offDayList.Add(new SelectListItem() { Value = "", Text = "请选择" });
            int days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            if (admin.IsPersonalTime && admin.WorkTime != null)
            {
                //var workDay = admin.WorkTime.WorkWeek;
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
                    //    if (monthList.Contains(currentMonth))
                    //    {
                    //        if (monthList.Contains(12) && monthList.Contains(1))
                    //        {
                    //        }
                    //        else
                    //        {
                    //            if (currentMonth > maxMonth)
                    //            {
                    //                currentMonth = maxMonth;
                    //            }
                    //            else
                    //            {
                    //                currentMonth = minMonth;
                    //            }
                    //        }
                    //    }
                }
                var WtdArry = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId && x.Year == currentYear
             && x.Month == currentMonth).ToList();
                if (WtdArry != null && WtdArry.Count() > 0)
                {
                    var dayArry = WtdArry.Where(w => w.WorkTimeType != 2).Select(w => w.WorkDay);
                    foreach (var item in dayArry)
                    {
                        if (Convert.ToInt32(item) > currentDay && Convert.ToInt32(item) <= days)
                        {
                            workDayList.Add(new SelectListItem() { Value = item.ToString(), Text = item + "号" });
                        }

                    }
                    var offDay = admin.WorkTime == null ? null : _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkTimeType == 2 &&
                          x.Year == currentYear && x.Month == currentMonth)
                        .Select(x => x.WorkDay);
                    if (offDay != null)
                    {
                        foreach (var item in offDay)
                        {
                            if (item > currentDay && item <= days)
                            {
                                offDayList.Add(new SelectListItem() { Value = item.ToString(), Text = item + "号" });
                            }
                        }
                    }
                }
            }
            var DepList = new List<SelectListItem>();
            ClassApplication ca = new ClassApplication();
            DepList.AddRange(_departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled).Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.DepartmentName
            }));
            ViewBag.workDayList = workDayList;
            ViewBag.offDayList = offDayList;
            ViewBag.DepList = DepList;
            ViewBag.RealName = AuthorityHelper.RealName;
            ViewBag.adminId = AuthorityHelper.OperatorId ?? 0;
            var classApplication = _classApplicationContract.ClassApplications.FirstOrDefault(x => x.Id == Id);
            return PartialView(classApplication);
        }
        [HttpPost]
        public JsonResult Update(ClassApplicationDto dto)
        {
            OperationResult oper = _classApplicationContract.Update(dto);
            return Json(oper);
        }

        public JsonResult Remove(string Idstr, bool isdelete)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(Idstr))
            {
                oper = _classApplicationContract.Remove(isdelete, Idstr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                    ToList().ConvertAll(x => Convert.ToInt32(x)).ToArray());
            }
            return Json(oper);
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
                return "0";
                //throw ex; //这里可以定义你自己的异常处理
            }
        }

        public ActionResult View(int Id)
        {
            var classApplication = _classApplicationContract.ClassApplications.FirstOrDefault(x => x.Id == Id);
            ViewBag.DepartmentName = classApplication.SuccessionDep.DepartmentName;
            ViewBag.SuccessionName = classApplication.Succession.Member.RealName;
            ViewBag.RealName = AuthorityHelper.RealName;
            ViewBag.adminId = AuthorityHelper.OperatorId ?? 0;
            return PartialView(classApplication);
        }

        public JsonResult CheckIsPersonTime()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var admin = _administratorContract.View(adminId);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "用户未登录"));
            }

            if (!admin.IsPersonalTime)
            {
                return Json(new OperationResult(OperationResultType.Error, "职位时间无法申请调班"));
            }
            return Json(new OperationResult(OperationResultType.Success));
        }
    }
}