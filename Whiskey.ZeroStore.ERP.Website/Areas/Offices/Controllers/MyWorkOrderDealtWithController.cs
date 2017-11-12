using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using System.Threading.Tasks;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;
using System.Collections.Generic;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class MyWorkOrderDealtWithController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MyWorkOrderDealtWithController));

        protected readonly IWorkOrderDealtWithContract _workOrderDealtWithContract;

        protected readonly IWorkOrderContract _workOrderContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IWorkOrderCategoryContract _workOrderCategoryContract;

        public MyWorkOrderDealtWithController(
            IWorkOrderDealtWithContract workOrderDealtWithContract,
            IWorkOrderContract workOrderContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContrac,
            IWorkOrderCategoryContract workOrderCategoryContract
            )
        {
            this._workOrderDealtWithContract = workOrderDealtWithContract;
            this._workOrderContract = workOrderContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContrac;
            this._workOrderCategoryContract = workOrderCategoryContract;
        }
        #endregion

        #region 初始化页面
        [Layout]
        public ActionResult Index()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            ViewBag.AdminId = adminId;
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
            string departmentName = "";
            FilterRule ruleDepartment = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "Department");
            if (ruleDepartment != null)
            {
                departmentName = ruleDepartment.Value.ToString();
                request.FilterGroup.Rules.Remove(ruleDepartment);
            }
            string workOrderTitlee = "";
            FilterRule ruleWorkOrderTitle = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "WorkOrderTitle");
            if (ruleWorkOrderTitle != null)
            {
                workOrderTitlee = ruleWorkOrderTitle.Value.ToString();
                request.FilterGroup.Rules.Remove(ruleWorkOrderTitle);
            }
            int workOrderCategoryId = 0;
            FilterRule ruleWorkOrderCategoryId = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "WorkOrderCategoryId");
            if (ruleWorkOrderCategoryId != null)
            {
                workOrderCategoryId = ruleWorkOrderCategoryId.Value != null ? Convert.ToInt32(ruleWorkOrderCategoryId.Value) : 0;
                request.FilterGroup.Rules.Remove(ruleWorkOrderCategoryId);
            }
            Expression<Func<WorkOrderDealtWith, bool>> predicate = FilterHelper.GetExpression<WorkOrderDealtWith>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(applicantName))
                {
                    predicate = predicate.And(p => p.WorkOrder != null && p.WorkOrder.Applicant != null && p.WorkOrder.Applicant.Member != null && p.WorkOrder.Applicant.Member.RealName.Contains(applicantName));
                }

                if (!string.IsNullOrEmpty(departmentName))
                {
                    predicate = predicate.And(p => p.WorkOrder != null && p.WorkOrder.DepartmentId.Equals(departmentName));
                }

                if (!string.IsNullOrEmpty(workOrderTitlee))
                {
                    predicate = predicate.And(p => p.WorkOrder != null && p.WorkOrder.WorkOrderTitle.Contains(workOrderTitlee));
                }

                if (workOrderCategoryId != 0)
                {
                    predicate = predicate.And(p => p.WorkOrder != null && p.WorkOrder.WorkOrderCategoryId.Equals(workOrderCategoryId));
                }
                int count;
                var list = (from s in _workOrderDealtWithContract.Entities.Where<WorkOrderDealtWith, int>(predicate, request.PageCondition, out count)
                            select new
                            {
                                s.Id,
                                s.IsDeleted,
                                s.IsEnabled,
                                s.WorkOrder.CreatedTime,
                                s.WorkOrder.WorkOrderTitle,
                                s.Status,
                                DepartmentName = s.WorkOrder.Department != null ? s.WorkOrder.Department.DepartmentName : "",
                                ApplicantName = s.WorkOrder.Applicant != null && s.WorkOrder.Applicant.Member != null ? s.WorkOrder.Applicant.Member.RealName : "",
                                WorkOrderCategoryName = s.WorkOrder.WorkOrderCategory != null ? s.WorkOrder.WorkOrderCategory.WorkOrderCategoryName : "",
                                OperationName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : "",
                                HandleName = s.WorkOrder.Handler != null && s.WorkOrder.Handler.Member != null ? s.WorkOrder.Handler.Member.RealName : "",
                                s.WorkOrder.DealtTime,
                                s.WorkOrder.FinishTime
                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
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
        public ActionResult Remove(int[] Id)
        {
            var result = _workOrderDealtWithContract.DeleteOrRecovery(true, Id);
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
        public ActionResult Recovery(int[] Id)
        {
            var result = _workOrderDealtWithContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 拒绝工单
        /// <summary>
        /// 拒绝工单
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="status">要进行操作的状态（-1：拒绝；1：接受；2：完成）</param>
        /// <returns></returns>
        [Log]
        public ActionResult NoPass(int Id)
        {
            var dto = _workOrderDealtWithContract.Edit(Id);
            return PartialView(dto);
        }
        /// <summary>
        /// 拒绝工单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult NoPass(WorkOrderDealtWithDto dealtwith)
        {
            var result = _workOrderDealtWithContract.DealtWith(dealtwith.Id, -1, dealtwith.Notes);

            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(dealtwith.Id);

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_o = "您指派给" + (wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单被拒绝处理，拒绝原因：" + wodt.Notes;
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_o, sendNotificationAction);
            }
            string msg = "您申请的工单被" + (wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "拒绝处理，拒绝原因：" + wodt.Notes;
            int applicantId = wodt.WorkOrder.ApplicantId;
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 接受工单
        /// <summary>
        /// 接受工单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Pass(int Id)
        {
            var result = _workOrderDealtWithContract.DealtWith(Id, 1, "");

            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(Id);

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_o = "您指派给" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单已被接受";
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_o, sendNotificationAction);
            }
            string msg = "您申请的工单已被" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "接受";
            int applicantId = wodt.WorkOrder.ApplicantId;
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 完成工单
        /// <summary>
        /// 完成工单
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult Finish(int Id)
        {
            var result = _workOrderDealtWithContract.DealtWith(Id, 2, "");

            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(Id);

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_o = "您指派给" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单已被完成";
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_o, sendNotificationAction);
            }
            string msg = "您申请的工单已被" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "完成";
            int applicantId = wodt.WorkOrder.ApplicantId;
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);

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

        #region APP
        #region 获取指派给用户的所有工单信息
        public ActionResult GetWorkOrderDealtWithsByAdminId(int adminId, int pageIndex, int pageSize, int status = -99)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (adminId <= 0)
            {
                opera.Message = "请重新登陆";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            if (status == -99)
            {
                int count = _workOrderDealtWithContract.Entities.Count(w => w.HandlerID == adminId && !w.IsDeleted && w.IsEnabled);
                var list = _workOrderDealtWithContract.Entities.Where(w => w.HandlerID == adminId && !w.IsDeleted && w.IsEnabled).OrderByDescending(w => w.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(s =>
                               new
                               {
                                   s.Id,
                                   CreatedTime = s.WorkOrder.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                                   s.WorkOrder.WorkOrderTitle,
                                   Status = s.WorkOrder.Status == -1 ? -2 : s.Status,
                                   s.WorkOrder.DepartmentId,
                                   DepartmentName = s.WorkOrder.Department != null ? s.WorkOrder.Department.DepartmentName : "",
                                   ApplicantName = s.WorkOrder.Applicant != null && s.WorkOrder.Applicant.Member != null ? s.WorkOrder.Applicant.Member.RealName : "",
                                   s.WorkOrder.WorkOrderCategoryId,
                                   WorkOrderCategoryName = s.WorkOrder.WorkOrderCategory != null ? s.WorkOrder.WorkOrderCategory.WorkOrderCategoryName : "",
                                   OperationName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : "",
                                   s.WorkOrder.HandlerID,
                                   HandleName = s.WorkOrder.Handler != null && s.WorkOrder.Handler.Member != null ? s.WorkOrder.Handler.Member.RealName : "",
                                   s.WorkOrder.Content,
                                   DealtTime = s.WorkOrder.DealtTime != null ? Convert.ToDateTime(s.WorkOrder.DealtTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                                   FinishTime = s.WorkOrder.FinishTime != null ? Convert.ToDateTime(s.WorkOrder.FinishTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                                   s.WorkOrder.ImgAddress
                               }).ToList();
                opera.Data = list;
                opera.Other = count;
            }
            else
            {
                int count = _workOrderDealtWithContract.Entities.Count(w => w.HandlerID == adminId && !w.IsDeleted && w.IsEnabled && w.Status == status);
                var list = _workOrderDealtWithContract.Entities.Where(w => w.HandlerID == adminId && !w.IsDeleted && w.IsEnabled && w.Status == status).OrderByDescending(w => w.CreatedTime).Skip((pageIndex - 1) * pageSize).ToList().Take(pageSize).Select(s =>
                               new
                               {
                                   s.Id,
                                   CreatedTime = s.WorkOrder.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                                   s.WorkOrder.WorkOrderTitle,
                                   Status = s.WorkOrder.Status == -1 ? -2 : s.Status,
                                   s.WorkOrder.DepartmentId,
                                   DepartmentName = s.WorkOrder.Department != null ? s.WorkOrder.Department.DepartmentName : "",
                                   ApplicantName = s.WorkOrder.Applicant != null && s.WorkOrder.Applicant.Member != null ? s.WorkOrder.Applicant.Member.RealName : "",
                                   s.WorkOrder.WorkOrderCategoryId,
                                   WorkOrderCategoryName = s.WorkOrder.WorkOrderCategory != null ? s.WorkOrder.WorkOrderCategory.WorkOrderCategoryName : "",
                                   OperationName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : "",
                                   s.WorkOrder.HandlerID,
                                   HandleName = s.WorkOrder.Handler != null && s.WorkOrder.Handler.Member != null ? s.WorkOrder.Handler.Member.RealName : "",
                                   s.WorkOrder.Content,
                                   DealtTime = s.WorkOrder.DealtTime != null ? Convert.ToDateTime(s.WorkOrder.DealtTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                                   FinishTime = s.WorkOrder.FinishTime != null ? Convert.ToDateTime(s.WorkOrder.FinishTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                                   s.WorkOrder.ImgAddress
                               }).ToList();
                opera.Data = list;
                opera.Other = count;
            }
            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取指派给用户的单个工单信息
        public ActionResult GetWorkOrderDealtWithById(int adminId, int workOrderDealtWithId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (adminId <= 0)
            {
                opera.Message = "请重新登陆";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            var entities = _workOrderDealtWithContract.Entities.Where(w => w.Id == workOrderDealtWithId && !w.IsDeleted && w.IsEnabled).ToList().Select(s =>
                      new
                      {
                          s.Id,
                          CreatedTime = s.WorkOrder.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                          s.WorkOrder.WorkOrderTitle,
                          Status = s.WorkOrder.Status == -1 ? -2 : s.Status,
                          s.WorkOrder.DepartmentId,
                          DepartmentName = s.WorkOrder.Department != null ? s.WorkOrder.Department.DepartmentName : "",
                          ApplicantName = s.WorkOrder.Applicant != null && s.WorkOrder.Applicant.Member != null ? s.WorkOrder.Applicant.Member.RealName : "",
                          s.WorkOrder.WorkOrderCategoryId,
                          WorkOrderCategoryName = s.WorkOrder.WorkOrderCategory != null ? s.WorkOrder.WorkOrderCategory.WorkOrderCategoryName : "",
                          OperationName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : "",
                          s.WorkOrder.HandlerID,
                          HandleName = s.WorkOrder.Handler != null && s.WorkOrder.Handler.Member != null ? s.WorkOrder.Handler.Member.RealName : "",
                          s.WorkOrder.Content,
                          DealtTime = s.WorkOrder.DealtTime != null ? Convert.ToDateTime(s.WorkOrder.DealtTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                          FinishTime = s.WorkOrder.FinishTime != null ? Convert.ToDateTime(s.WorkOrder.FinishTime).ToString("yyyy-MM-dd hh:mm:ss") : "",
                          s.WorkOrder.ImgAddress
                      }).ToList();
            opera.Data = entities;
            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 拒绝接受工单
        public JsonResult NoPassWorkOrder(int Id, string notes, int adminId)
        {
            if (!_administratorContract.CheckExists(p => p.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"), JsonRequestBehavior.AllowGet);
            }
            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(Id);

            if (wodt == null || wodt.HandlerID != adminId)
            {
                return Json(new OperationResult(OperationResultType.Error, "无法操作"), JsonRequestBehavior.AllowGet);
            }
            var result = _workOrderDealtWithContract.DealtWith(Id, -1, notes);

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_o = "您指派给" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单被拒绝处理，拒绝原因：" + wodt.Notes;
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_o, sendNotificationAction);
            }
            int applicantId = wodt.WorkOrder.ApplicantId;
            string msg = "您申请的工单被" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "拒绝处理，拒绝原因：" + wodt.Notes;
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 接受工单
        public JsonResult PassWorkOrder(int Id, int adminId)
        {
            if (!_administratorContract.CheckExists(p => p.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"), JsonRequestBehavior.AllowGet);
            }
            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(Id);

            if (wodt == null || wodt.HandlerID != adminId)
            {
                return Json(new OperationResult(OperationResultType.Error, "无法操作"), JsonRequestBehavior.AllowGet);
            }
            var result = _workOrderDealtWithContract.DealtWith(Id, 1, "");

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_0 = "您指派给" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单已被接受";
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_0, sendNotificationAction);
            }
            int applicantId = wodt.WorkOrder.ApplicantId;
            string msg = "您申请的工单已被" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "接受";
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 完成工单
        public JsonResult FinishWorkOrder(int Id, int adminId)
        {
            if (!_administratorContract.CheckExists(p => p.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"), JsonRequestBehavior.AllowGet);
            }
            WorkOrderDealtWith wodt = _workOrderDealtWithContract.View(Id);

            if (wodt == null || wodt.HandlerID != adminId)
            {
                return Json(new OperationResult(OperationResultType.Error, "无法操作"), JsonRequestBehavior.AllowGet);
            }
            var result = _workOrderDealtWithContract.DealtWith(Id, 2, "");

            int operationId = wodt.WorkOrder.OperatorId ?? 0;
            if (operationId > 0)
            {
                string msg_o = "您指派给" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "的工单已被完成";
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg_o, sendNotificationAction);
            }
            int applicantId = wodt.WorkOrder.ApplicantId;
            string msg = "您申请的工单已被" + (wodt.Handler != null && wodt.Handler.Member != null ? wodt.Handler.Member.RealName : "") + "完成";
            EntityContract._notificationContract.SendNotice(applicantId, "工单状态更新提醒", msg, sendNotificationAction);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion
        
        public JsonResult UnfinishedWorkOrderCount()
        {
            var count = (from d in _workOrderDealtWithContract.Entities
                         join w in _workOrderContract.Entities on d.WorkOrderId equals w.Id
                         where d.HandlerID == (AuthorityHelper.OperatorId ?? 0) && (d.Status == 0 || d.Status == 1) && (w.Status == 1 || w.Status == 2) && !d.IsDeleted && d.IsEnabled
                         && !w.IsDeleted && w.IsEnabled
                         select 1).Count();
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, count));
        }
    }
}

