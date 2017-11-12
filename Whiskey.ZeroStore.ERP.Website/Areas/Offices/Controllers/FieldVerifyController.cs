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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class FieldVerifyController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceController));
        
        protected readonly IAdministratorContract _administratorContract;
        
        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;

        protected readonly IFieldContract _fieldContract;

        protected readonly IWorkTimeContract _workTimeContract;
        protected readonly IMemberContract _memberContract;
        public FieldVerifyController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract,
            IFieldContract fieldContract,
            IWorkTimeContract workTimeContract,
            IMemberContract memberContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
            _fieldContract = fieldContract;
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

        #region 获取考勤数据列表
        /// <summary>
        /// 获取考勤数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            int adminId = AuthorityHelper.OperatorId ?? 0;
            string memberName = Request["memberName"];
            int departmentId = Request["DepartmentId"] == "" ? 0 : Convert.ToInt32(Request["DepartmentId"]);
            Expression<Func<Field, bool>> predicate = FilterHelper.GetExpression<Field>(request.FilterGroup);
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

                IQueryable<Field> listField = _fieldContract.Fields.Where(x => x.IsDeleted == false && x.IsEnabled == true).OrderByDescending(x => x.CreatedTime);
                listField = listField.Where(x => listAdminId.Contains(x.AdminId));
                var list = listField.Where<Field, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    x.Admin.Member.RealName,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    x.Admin.Department.DepartmentName,
                    x.VerifyType,
                    x.FieldDays,
                    x.FieldWorkDays,
                    x.IsEnabled,
                    x.IsDeleted,   
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 审核数据
        public ActionResult Update(int Id)
        {
            var dto = _fieldContract.Edit(Id);
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult Update(FieldDto dto)
        {
            dto.VerifyAdminId = AuthorityHelper.OperatorId;
            var res = _fieldContract.Verify(dto);
            string content = string.Empty;
            string title = "外勤通知";
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

                Field info = _fieldContract.Fields.Where(x => x.Id == item).FirstOrDefault();
                if (info != null && info.VerifyType == 0)
                {
                    FieldDto dto = new FieldDto();
                    dto.Id = item;
                    dto.FieldReason = info.FieldReason;
                    dto.StartTime = info.StartTime;
                    dto.EndTime = info.EndTime;
                    dto.FieldDays = info.FieldDays;
                    dto.FieldWorkDays = info.FieldWorkDays;
                    dto.VerifyType = verifyType;
                    dto.AdminId = info.AdminId;
                    dto.VerifyAdminId = AuthorityHelper.OperatorId;
                    res = _fieldContract.Verify(dto);
                    if (res.ResultType == OperationResultType.Success)
                    {
                        countSuccess++;
                        string content = string.Empty;
                        string title = "请假通知";
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
        public JsonResult FiledCount()
        {
            var listDeps = Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false).Select(s => s.Value).ToList().ConvertAll(c => Convert.ToInt32(c));
            var count = 0;
            var query = _fieldContract.Fields;
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