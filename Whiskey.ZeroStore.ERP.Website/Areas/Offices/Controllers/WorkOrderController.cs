using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class WorkOrderController : BaseController
    {

        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkOrderController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IWorkOrderContract _workOrderContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IWorkOrderCategoryContract _workOrderCategoryContract;
        protected readonly INotificationContract _notificationContract;

        public WorkOrderController(
            IWorkOrderContract workOrderContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IWorkOrderCategoryContract workOrderCategoryContract,
            INotificationContract notificationContract
            )
        {
            this._workOrderContract = workOrderContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContract;
            this._workOrderCategoryContract = workOrderCategoryContract;
            this._notificationContract = notificationContract;
        }
        #endregion

        #region 初始化页面
        [Layout]
        public ActionResult Index()
        {
            ViewBag.Departments = GetDepartments(true);
            ViewBag.WorkOrderCategorys = GetWorkOrderCategorys();
            return View();
        }
        #endregion

        #region 查看数据
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _workOrderContract.View(Id);
            ViewBag.ApplicantName = result.Applicant != null && result.Applicant.Member != null ? result.Applicant.Member.RealName : "";
            ViewBag.DepartmentName = result.Department != null ? result.Department.DepartmentName : "";
            ViewBag.WorkOrderCategoryName = result.WorkOrderCategory != null ? result.WorkOrderCategory.WorkOrderCategoryName : "";
            string status = "";
            switch (result.Status)
            {
                case -1:
                    status = "已撤销";
                    break;
                case 0:
                    status = "指派中";
                    break;
                case 1:
                    status = "已分配";
                    break;
                case 2:
                    status = "已接受";
                    break;
                case 3:
                    status = "已完成";
                    break;
                default:
                    break;
            }
            ViewBag.Status = status;
            ViewBag.HandlerName = result.Handler != null && result.Handler.Member != null ? result.Handler.Member.RealName : "";
            ViewBag.OperatorName = result.Operator != null && result.Operator.Member != null ? result.Operator.Member.RealName : "";
            return PartialView(result);
        }
        #endregion

        #region 指派工单
        /// <summary>
        /// 指派工单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Assign(int Id)
        {
            ViewBag.Id = Id;
            return PartialView();
        }
        /// <summary>
        /// 指派工单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Assign(int AdminIds, WorkOrderDto dto)
        {
            if (AdminIds == 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请选择员工"));
            }
            Administrator admin = _administratorContract.View(AdminIds);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "您选择的员工不存在"));
            }
            OperationResult opera = _workOrderContract.Assign(dto.Id, AdminIds);
            EntityContract._notificationContract.SendNotice(AdminIds, "指派工单", "您有新的被指派工单哦~~", sendNotificationAction);
            string msg = "您申请的工单已被指派给" + (admin.Member != null ? admin.Member.RealName : "");
            int applicantId = _workOrderContract.View(dto.Id).ApplicantId;
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);

            string applicantName = "";
            FilterRule ruleApplicantName = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "ApplicantName");
            if (ruleApplicantName != null)
            {
                applicantName = ruleApplicantName.Value.ToString();
                request.FilterGroup.Rules.Remove(ruleApplicantName);
            }
            string handleName = "";
            FilterRule ruleHandleName = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "HandleName");
            if (ruleHandleName != null)
            {
                handleName = ruleHandleName.Value.ToString();
                request.FilterGroup.Rules.Remove(ruleHandleName);
            }
            string departmentName = "";
            FilterRule ruleDepartment = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "Department");
            if (ruleDepartment != null)
            {
                departmentName = ruleDepartment.Value.ToString();
                request.FilterGroup.Rules.Remove(ruleDepartment);
            }
            Expression<Func<WorkOrder, bool>> predicate = FilterHelper.GetExpression<WorkOrder>(request.FilterGroup);

            var data = await Task.Run(() =>
            {

                if (!string.IsNullOrEmpty(applicantName))
                {
                    predicate = predicate.And(p => p.Applicant != null && p.Applicant.Member != null && p.Applicant.Member.RealName.Contains(applicantName));
                }

                if (!string.IsNullOrEmpty(handleName))
                {
                    predicate = predicate.And(p => p.Handler != null && p.Handler.Member != null && p.Handler.Member.RealName.Contains(handleName));
                }

                if (!string.IsNullOrEmpty(departmentName))
                {
                    predicate = predicate.And(p => p.DepartmentId.Equals(departmentName));
                }
                int count;
                var list = (from s in _workOrderContract.Entities.Where<WorkOrder, int>(predicate, request.PageCondition, out count)
                            select new
                            {
                                s.Id,
                                s.IsDeleted,
                                s.IsEnabled,
                                s.CreatedTime,
                                s.WorkOrderTitle,
                                s.Status,
                                DepartmentName = s.Department != null ? s.Department.DepartmentName : "",
                                ApplicantName = s.Applicant != null && s.Applicant.Member != null ? s.Applicant.Member.RealName : "",
                                WorkOrderCategoryName = s.WorkOrderCategory != null ? s.WorkOrderCategory.WorkOrderCategoryName : "",
                                OperationName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : "",
                                HandleName = s.Handler != null && s.Handler.Member != null ? s.Handler.Member.RealName : "",
                                s.DealtTime,
                                s.FinishTime
                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            list.AddRange(Utils.GetCurUserDepartList(AuthorityHelper.OperatorId, _administratorContract, false));
            ViewBag.depList = list;
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.Where(r => r.Field == "RealName").FirstOrDefault();
            if (rule != null)
            {
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            if (rule != null)
            {
                string realName = rule.Value.ToString();
                predicate = predicate.And(a => a.Member.RealName.Contains(realName));
            }
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _administratorContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(a => new
                {
                    a.Id,
                    a.Member.RealName
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAdminListByDepartmentId(int DepartmentId)
        {
            if (DepartmentId > 0)
            {
                var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled && a.DepartmentId == DepartmentId).Select(m => new
                {
                    m.Id,
                    m.Member.RealName,
                }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = _administratorContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(m => new
                {
                    m.Id,
                    m.Member.RealName,
                }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Remove(int[] Id)
        {
            var result = _workOrderContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Recovery(int[] Id)
        {
            var result = _workOrderContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取部门
        /// <summary>
        /// 获取管理的部门
        /// </summary>
        /// <param name="opera"></param>
        /// <returns></returns>
        private List<SelectListItem> GetDepartments(bool IsAll = false)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _administratorContract.View(adminId);
            List<SelectListItem> select = new List<SelectListItem>();
            select.Add(new SelectListItem() { Value = "", Text = "请选择" });
            if (admin == null)
            {
                return select;
            }
            List<int> listId = new List<int>();
            List<Department> listDepart = new List<Department>();
            if (admin.JobPosition != null && admin.JobPosition.IsLeader == true)
            {
                listId.Add(admin.DepartmentId ?? 0);
                listDepart.Add(admin.Department);
            }
            if (!IsAll)
            {
                Department depart = _departmentContract.Departments.FirstOrDefault(x => !x.IsDeleted && x.IsEnabled && x.SubordinateId == adminId);
                if (depart != null)
                {
                    listDepart.AddRange(depart.Children.ToList());
                }
            }
            else
            {
                listDepart = _departmentContract.Departments.Where(x => !x.IsDeleted && x.IsEnabled).ToList();
            }
            if (listDepart != null)
            {
                select.AddRange(listDepart.Select(x => new SelectListItem()
                {
                    Value = x.Id.ToString(),
                    Text = x.DepartmentName,
                }).ToList());
            }
            return select;
        }
        #endregion

        #region 获取工单类型
        public List<SelectListItem> GetWorkOrderCategorys()
        {
            List<int> listId = new List<int>();
            IQueryable<WorkOrderCategory> listWOC = _workOrderCategoryContract.WorkOrderCategorys.Where(w => !w.IsDeleted && w.IsEnabled);

            List<SelectListItem> select = new List<SelectListItem>();
            select.Add(new SelectListItem() { Value = "", Text = "请选择" });

            if (listWOC == null)
            {
                return select;
            }
            select.AddRange(listWOC.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = x.WorkOrderCategoryName,
            }).ToList());
            return select;
        }
        /// <summary>
        /// app使用
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAllWorkOrderCategorys()
        {
            var listWOC = _workOrderCategoryContract.WorkOrderCategorys.Where(w => !w.IsDeleted && w.IsEnabled).Select(w => new
            {
                w.Id,
                w.WorkOrderCategoryName
            }).ToList();
            OperationResult opera = new OperationResult(OperationResultType.Success, "", listWOC);
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult WorkOrderCount()
        {
            int count = _workOrderContract.Entities.Count(w => !w.IsDeleted && w.IsEnabled && w.Status == 0);
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        }
    }
}

