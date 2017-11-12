using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Web.Mvc;
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
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Linq.Expressions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class ToExamineController : BaseController
    {
        private readonly IClassApplicationContract _classApplicationContract;
        private readonly IWorkTimeDetaileContract _workTimeDetaileContract;
        private readonly IAdministratorContract _administratorContract;
        private readonly IDepartmentContract _departmentContract;
        private readonly IWorkTimeContract _workTimeContract;
        public ToExamineController(IClassApplicationContract classApplicationContract, IWorkTimeDetaileContract workTimeDetaileContract,
    IAdministratorContract administratorContract, IDepartmentContract departmentContract, IWorkTimeContract workTimeContract)
        {
            _classApplicationContract = classApplicationContract;
            _administratorContract = administratorContract;
            _workTimeDetaileContract = workTimeDetaileContract;
            _departmentContract = departmentContract;
            _workTimeContract = workTimeContract;
        }
        [Layout]
        public ActionResult Index()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            ViewBag.AdminId = adminId;

            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.depList = list;
            return View();
        }
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string SuccessionName = Request["SuccessionName"];
            string ToExamineResult = Request["ToExamineResult"];
            string DepartmentId = Request["DepartmentId"];
            Expression<Func<ClassApplication, bool>> predicate = FilterHelper.GetExpression<ClassApplication>(request.FilterGroup);
            int PageIndex = request.PageCondition.PageIndex + 1;
            int PageSize = request.PageCondition.PageSize;
            var listData = _classApplicationContract.ClassApplications.Where(predicate);
            var depId_list = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(x => Convert.ToInt32(x.Value)).ToList();
            if (depId_list.Count() > 0)
            {
                listData = listData.Where(x => depId_list.Contains(x.Admin.DepartmentId.Value));
            }
            if (!string.IsNullOrEmpty(DepartmentId))
            {
                int depId = Convert.ToInt32(DepartmentId);
                listData = listData.Where(x => x.Admin.DepartmentId.Value == depId);
            }
            if (!string.IsNullOrEmpty(ToExamineResult))
            {
                int toExamine = Convert.ToInt32(ToExamineResult);
                listData = listData.Where(x => x.ToExamineResult == toExamine);
            }
            var data = await Task.Run(() =>
            {
                var count = 0;
                listData = listData.Where(x => x.Admin.Member.RealName.Contains(SuccessionName));
                count = listData.Count();
                var list = listData.OrderByDescending(c => c.CreatedTime).Skip(request.PageCondition.PageIndex * PageSize).Take(PageIndex * PageSize).Select(x => new
                {
                    x.Id,
                    admin_DepartmentName = x.Admin.Department.DepartmentName,
                    x.Admin.Member.RealName,
                    x.CreatedTime,
                    x.Day,
                    x.OffDay,
                    x.SuccessionDep.DepartmentName,
                    x.UpdatedTime,
                    x.ToExamineResult,
                    x.IsDeleted,
                    x.AdminId,
                    SuccessionName = x.Succession.Member.RealName
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToExamineHander(int ToExamineStatues, int Id)
        {
            var oper = _classApplicationContract.ToExamine(ToExamineStatues, Id);
            if (ToExamineStatues != 1 || oper.ResultType == OperationResultType.Error)
            {
                return Json(oper);
            }
            var class_Applcation = _classApplicationContract.ClassApplications.FirstOrDefault(x => x.Id == Id);
            var admin = class_Applcation.Admin;
            if (admin.WorkTime == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "暂无工作时间"));
            }
            var work_time = _workTimeContract.WorkTimes.FirstOrDefault(x => x.Id == class_Applcation.Admin.WorkTime.Id);
            var workTimrId = class_Applcation.Admin.WorkTime.Id;
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            var monthList = _workTimeDetaileContract.WorkTimeDetailes.Where(x => x.WorkTimeId == workTimrId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
            if (monthList.Count == 2)
            {
                int minMonth = monthList.Min();
                int minYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == minMonth).Year;
                int maxMonth = monthList.Max();
                int maxYear = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == workTimrId && x.Month == maxMonth).Year;

                if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                {
                    if (minYear > maxYear)
                    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                        currentYear = maxYear;
                        currentMonth = maxMonth;
                    }
                    else
                    {
                        currentYear = minYear;
                        currentMonth = minMonth;
                    }
                }

                //if (monthList.Contains(currentMonth))
                //{
                //if ((minMonth == currentMonth && minYear == currentYear) || (maxMonth == currentMonth && maxYear == currentYear))
                //{
                //    //if (monthList.Contains(12) && monthList.Contains(1))
                //    //{
                //    //}
                //    if (minYear > maxYear)
                //    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                //        if (maxMonth == currentMonth)
                //        {//若当月为12月，则下月为一月份
                //            currentMonth = minMonth;
                //            currentYear = minYear;
                //        }
                //        else
                //        {//若当月为1月，且未进行下个月的排班，则下月用最小月（此处即maxMonth）排班
                //            currentMonth = maxMonth;
                //            currentYear = maxYear;
                //        }
                //    }
                //    else
                //    {
                //        if (minYear > maxYear)
                //        {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                //            currentMonth = maxMonth;
                //            currentYear = maxYear;
                //        }
                //        else
                //        {
                //            currentMonth = minMonth;
                //            currentYear = minYear;
                //        }
                //    }
                //}
                //else
                //{
                //    currentMonth = minMonth;
                //}
            }
            oper = _administratorContract.Update(admin);
            var dayId = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkDay == class_Applcation.Day &&
            x.Year == currentYear && x.Month == currentMonth);
            var offdayId = _workTimeDetaileContract.WorkTimeDetailes.FirstOrDefault(x => x.WorkTimeId == admin.WorkTime.Id && x.WorkDay == class_Applcation.OffDay
            && x.Year == currentYear && x.Month == currentMonth);
            if (oper.ResultType == OperationResultType.Error)
            {
                return Json(oper);
            }

            WorkTimeDetaileDto daydetail = _workTimeDetaileContract.Edit(dayId.Id);
            daydetail.AmStartTime = offdayId.AmStartTime;
            daydetail.AmEndTime = offdayId.AmEndTime;
            daydetail.PmStartTime = offdayId.PmStartTime;
            daydetail.PmEndTime = offdayId.PmEndTime;
            daydetail.WorkHour = offdayId.WorkHour;
            daydetail.WorkTimeType = offdayId.WorkTimeType;

            //oper = _workTimeDetaileContract.UpdateWorkType(2, class_Applcation.OffDay, dayId == null ? 0 : dayId.Id);
            if (oper.ResultType == OperationResultType.Error)
            {
                return Json(oper);
            }
            WorkTimeDetaileDto offdaydetail = _workTimeDetaileContract.Edit(offdayId.Id);
            offdaydetail.AmStartTime = dayId.AmStartTime;
            offdaydetail.AmEndTime = dayId.AmEndTime;
            offdaydetail.PmStartTime = dayId.PmStartTime;
            offdaydetail.PmEndTime = dayId.PmEndTime;
            offdaydetail.WorkHour = dayId.WorkHour;
            offdaydetail.WorkTimeType = dayId.WorkTimeType;

            oper = _workTimeDetaileContract.Update(daydetail);
            oper = _workTimeDetaileContract.Update(offdaydetail);
            //oper = _workTimeDetaileContract.UpdateWorkType(0, class_Applcation.Day, offdayId == null ? 0 : offdayId.Id);

            return Json(oper);
        }

        //    return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        public JsonResult ShiftAuditCount()
        {
            var listData = _classApplicationContract.ClassApplications.Where(x => !x.IsDeleted && x.IsEnabled);
            var depId_list = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(x => Convert.ToInt32(x.Value)).ToList();
            listData = listData.Where(x => depId_list.Contains(x.Admin.DepartmentId.Value) && x.ToExamineResult == 0);
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, listData.Count()));
        }
    }
}