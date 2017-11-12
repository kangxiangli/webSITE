﻿using System;
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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    //[License(CheckMode.Verify)]
    public class OvertimeVerifyController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly IOvertimeContract _overtimeContract;

        protected readonly IWorkTimeContract _workTimeContract;
        protected readonly IMemberContract _memberContract;

        public OvertimeVerifyController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            IOvertimeContract overtimeContract,
            IWorkTimeContract workTimeContract,
            IMemberContract memberContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _overtimeContract = overtimeContract;
            _workTimeContract = workTimeContract;
            _memberContract = memberContract;
        }
        #endregion

        #region 初始化界面

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
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string memberName = Request["memberName"];
            int departmentId = Request["DepartmentId"] == "" ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            Expression<Func<Overtime, bool>> predicate = FilterHelper.GetExpression<Overtime>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                Administrator admin = listAdmin.FirstOrDefault(x => x.Id == adminId);
                List<int> listAdminId = new List<int>();
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
                    if (departmentId != 0)
                    {
                        childrenId = listDepart.Where(x => x.Id == departmentId).Select(x => x.Id).ToList();
                    }
                    List<Administrator> listEntity = listAdmin.Where(x => childrenId.Contains(x.DepartmentId ?? 0)).ToList();
                    if (listEntity != null && listEntity.Count() > 0)
                    {
                        listAdminId.AddRange(listEntity.Select(x => x.Id).ToList());
                    }
                }
                IQueryable<Overtime> listOvertime = _overtimeContract.Overtimes.Where(x => x.IsDeleted == false && x.IsEnabled == true).OrderByDescending(x => x.CreatedTime);
                listOvertime = listOvertime.Where(x => listAdminId.Contains(x.AdminId));
                //    if (!string.IsNullOrEmpty(memberName))
                //    {
                //        listOvertime = from a in listOvertime
                //                       join b in _administratorContract.Administrators
                //on a.AdminId equals b.Id into a_bJion
                //                       from x in a_bJion.DefaultIfEmpty()
                //                       join c in _memberContract.Members
                //                 on x.MemberId equals c.Id into a_cJoin
                //                       from y in a_cJoin
                //                       where y.MemberName.Contains(memberName)
                //                       select a;

                //    }
                var list = listOvertime.Where<Overtime, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.VerifyType,
                    x.OvertimeDays,
                    x.Admin.Department.DepartmentName,
                    x.IsEnabled,
                    x.IsDeleted,
                    x.GetPoints
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 审核数据
        public ActionResult Update(int Id)
        {
            var dto = _overtimeContract.Edit(Id);
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult Update(OvertimeDto dto)
        {
            dto.VerifyAdminId = AuthorityHelper.OperatorId;
            var res = _overtimeContract.Verify(dto);
            string content = string.Empty;
            string title = "加班通知";
            if (dto.VerifyType == (int)VerifyFlag.NoPass)
            {
                content = "审核不通过";
            }
            else
            {
                content = "审核通过";
            }
            EntityContract._notificationContract.SendNotice(dto.AdminId, title, content, sendNotificationAction);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Verify(int verifyType, params int[] Ids)
        {
            var res = new OperationResult(OperationResultType.Error);
            int countSuccess = 0;
            int countFail = 0;
            foreach (var item in Ids)
            {

                Overtime info = _overtimeContract.Overtimes.Where(x => x.Id == item).FirstOrDefault();
                if (info != null && info.VerifyType == 0)
                {
                    OvertimeDto dto = new OvertimeDto();
                    dto.Id = item;
                    dto.OvertimeReason = info.OvertimeReason;
                    dto.StartTime = info.StartTime;
                    dto.EndTime = info.EndTime;
                    dto.OvertimeDays = info.OvertimeDays;
                    dto.VerifyType = verifyType;
                    dto.AdminId = info.AdminId;
                    dto.VerifyAdminId = AuthorityHelper.OperatorId;
                    res = _overtimeContract.Verify(dto);
                    if (res.ResultType == OperationResultType.Success)
                    {
                        countSuccess++;
                        string content = string.Empty;
                        string title = "加班通知";
                        if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            content = "审核不通过";
                        }
                        else
                        {
                            content = "审核通过";
                        }

                        EntityContract._notificationContract.SendNotice(dto.AdminId, title, content, sendNotificationAction);
                    }
                    else
                    {
                        countFail++;
                    }
                }
            }
            var strResult = string.Format("操作完成：成功{0}个，失败{1}个", countSuccess, countFail);
            var oper = new OperationResult(OperationResultType.Success, strResult);
            return Json(oper);
        }
        public JsonResult OvertimeCount()
        {
            var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            var count = 0;
            var query = _overtimeContract.Overtimes;
            var admin_list = _administratorContract.Administrators.Where(x => listDeps.Contains(x.DepartmentId.Value) && !x.IsDeleted && x.IsEnabled);
            var da = from a in query
                     join b in admin_list on a.AdminId equals b.Id
                     where a.IsEnabled && !a.IsDeleted && a.VerifyType == 0
                     select a;
            count = da.Count();
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        }
    }
}