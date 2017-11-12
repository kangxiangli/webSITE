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
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.IO;
using System.Web;
using System.Globalization;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class MyWorkOrderController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MyWorkOrderController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IWorkOrderContract _workOrderContract;

        protected readonly IWorkOrderDealtWithContract _workOrderDealtWithContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IWorkOrderCategoryContract _workOrderCategoryContract;

        public MyWorkOrderController(
            IWorkOrderContract workOrderContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IWorkOrderCategoryContract workOrderCategoryContract,
            IWorkOrderDealtWithContract workOrderDealtWithContract
            )
        {
            this._workOrderContract = workOrderContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContract;
            this._workOrderCategoryContract = workOrderCategoryContract;
            this._workOrderDealtWithContract = workOrderDealtWithContract;
        }
        #endregion


        #region 初始化页面
        [Layout]
        public ActionResult Index()
        {
            ViewBag.Departments = GetDepartments(true);
            ViewBag.WorkOrderCategorys = GetWorkOrderCategorys();
            ViewBag.adminId = AuthorityHelper.OperatorId ?? 0;
            return View();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.WorkOrderCategorys = GetWorkOrderCategorys();
            return PartialView();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Create(WorkOrderDto dto)
        {
            if (dto == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单申请"));
            }
            var admin = _administratorContract.View(AuthorityHelper.OperatorId ?? 0);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登陆"));
            }
            if (dto.WorkOrderTitle.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单标题"));
            }

            dto.DepartmentId = admin.DepartmentId;

            if (_workOrderContract.Entities.Count(w => w.DepartmentId == dto.DepartmentId && w.Status != -1 && w.Status != 3) > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该部门还有已申请工单未被处理，暂无法申请新的工单"), JsonRequestBehavior.AllowGet);
            }
            if (dto.WorkOrderCategoryId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请选择类型"));
            }
            if (dto.Content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单内容"));
            }
            if (dto.ImgAddress != null && dto.ImgAddress.Trim().Length > 0 && !FileHelper.FileIsExist(dto.ImgAddress))
            {
                return Json(new OperationResult(OperationResultType.Error, "图片不存在"));
            }
            dto.ApplicantId = AuthorityHelper.OperatorId ?? 0;

            var result = _workOrderContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Update(WorkOrderDto dto)
        {
            var workorder = _workOrderContract.Edit(dto.Id);
            if (AuthorityHelper.OperatorId == null || AuthorityHelper.OperatorId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登陆"), JsonRequestBehavior.AllowGet);
            }
            if (workorder == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "该工单不存在"), JsonRequestBehavior.AllowGet);
            }
            if (dto.WorkOrderTitle != workorder.WorkOrderTitle && dto.WorkOrderTitle.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单标题"), JsonRequestBehavior.AllowGet);
            }
            if (dto.WorkOrderCategoryId != workorder.WorkOrderCategoryId && dto.WorkOrderCategoryId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请选择类型"), JsonRequestBehavior.AllowGet);
            }
            if (dto.Content != dto.Content && workorder.Content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单内容"), JsonRequestBehavior.AllowGet);
            }
            if (dto.ImgAddress != null && dto.ImgAddress.Trim().Length > 0 && !FileHelper.FileIsExist(dto.ImgAddress))
            {
                return Json(new OperationResult(OperationResultType.Error, "图片不存在"), JsonRequestBehavior.AllowGet);
            }
            workorder.WorkOrderTitle = dto.WorkOrderTitle;
            workorder.WorkOrderCategoryId = dto.WorkOrderCategoryId;
            workorder.Content = dto.Content;
            if (dto.ImgAddress.Trim() != workorder.ImgAddress.Trim() && FileHelper.FileIsExist(dto.ImgAddress))
            {
                workorder.ImgAddress = dto.ImgAddress;
            }
            var result = _workOrderContract.Update(workorder);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _workOrderContract.Edit(Id);
            ViewBag.WorkOrderCategorys = GetWorkOrderCategorys();
            return PartialView(result);
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
        public ActionResult Recovery(int[] Id)
        {
            var result = _workOrderContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 撤销数据
        /// <summary>
        /// 撤销数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Cancel(int[] Id)
        {
            var result = _workOrderContract.CancelOrRecoveryCancel(true, Id);

            foreach (var id in Id)
            {
                var dto = _workOrderContract.View(id);
                var wodt = _workOrderDealtWithContract.Entities.FirstOrDefault(d => d.WorkOrderId == dto.Id && !d.IsDeleted && d.IsEnabled && (d.Status == 0 || d.Status == 1));

                int operationId = dto.OperatorId ?? 0;
                string msg = (dto.Applicant != null && dto.Applicant.Member != null ? dto.Applicant.Member.RealName : "") + "的工单已被撤销";
                if (operationId > 0)
                {
                    EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg, sendNotificationAction);
                }
                if (wodt != null)
                {
                    EntityContract._notificationContract.SendNotice(wodt.HandlerID, "工单状态更新提醒", msg, sendNotificationAction);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复撤销数据
        /// <summary>
        /// 恢复撤销数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult RecoveryCancel(int[] Id)
        {
            var result = _workOrderContract.CancelOrRecoveryCancel(false, Id);
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
        #region 申请工单
        public JsonResult CreateWorkOrder(int adminId, string title, int workOrderCategoryId, string content)
        {
            var admin = _administratorContract.View(adminId);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登陆"), JsonRequestBehavior.AllowGet);
            }

            if (_workOrderContract.Entities.Count(w => w.DepartmentId == admin.DepartmentId && w.Status != -1 && w.Status != 3) > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该部门还有已申请工单未被处理，暂无法申请新的工单"), JsonRequestBehavior.AllowGet);
            }

            if (title.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单标题"), JsonRequestBehavior.AllowGet);
            }
            if (workOrderCategoryId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请选择类型"), JsonRequestBehavior.AllowGet);
            }
            if (content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单内容"), JsonRequestBehavior.AllowGet);
            }



            WorkOrderDto dto = new WorkOrderDto();
            dto.ApplicantId = adminId;
            dto.WorkOrderTitle = title;
            dto.DepartmentId = admin.DepartmentId;
            dto.WorkOrderCategoryId = workOrderCategoryId;
            dto.Content = content;

            HttpFileCollectionBase files = Request.Files;
            if (files != null && files.Count > 0)
            {
                OperationResult oper = UploadImg(files);
                if (oper.ResultType == OperationResultType.Success)
                {
                    dto.ImgAddress = oper.Data.ToString();
                }
            }

            var result = _workOrderContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据
        public JsonResult UpdateWorkOrder(int workOrderId, int adminId, string title, int workOrderCategoryId, string content)
        {
            if (workOrderId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该工单不存在"), JsonRequestBehavior.AllowGet);
            }
            var dto = _workOrderContract.Edit(workOrderId);
            if (adminId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登陆"), JsonRequestBehavior.AllowGet);
            }
            if (title != dto.WorkOrderTitle && title.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单标题"), JsonRequestBehavior.AllowGet);
            }
            if (workOrderCategoryId != dto.WorkOrderCategoryId && workOrderCategoryId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请选择类型"), JsonRequestBehavior.AllowGet);
            }
            if (content != dto.Content && content.Trim() == "")
            {
                return Json(new OperationResult(OperationResultType.Error, "请填写工单内容"), JsonRequestBehavior.AllowGet);
            }
            dto.WorkOrderTitle = title;
            dto.WorkOrderCategoryId = workOrderCategoryId;
            dto.Content = content;

            HttpFileCollectionBase files = Request.Files;
            if (files != null && files.Count > 0)
            {
                OperationResult oper = UploadImg(files);
                if (oper.ResultType == OperationResultType.Success)
                {
                    dto.ImgAddress = oper.Data.ToString();
                }
            }
            var result = _workOrderContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 撤销工单
        public JsonResult CancelWorkOrder(int workOrderId, int adminId)
        {
            if (adminId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登陆"), JsonRequestBehavior.AllowGet);
            }
            if (workOrderId <= 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该工单不存在"), JsonRequestBehavior.AllowGet);
            }
            var dto = _workOrderContract.View(workOrderId);
            dto.Status = -1;
            var result = _workOrderContract.Update(dto);

            var wodt = _workOrderDealtWithContract.Entities.FirstOrDefault(d => d.WorkOrderId == dto.Id && !d.IsDeleted && d.IsEnabled && (d.Status == 0 || d.Status == 1));

            int operationId = dto.OperatorId ?? 0;
            string msg = (dto.Applicant != null && dto.Applicant.Member != null ? dto.Applicant.Member.RealName : "") + "的工单已被撤销";
            if (operationId > 0)
            {
                EntityContract._notificationContract.SendNotice(operationId, "工单状态更新提醒", msg, sendNotificationAction);
            }
            if (wodt != null)
            {
                EntityContract._notificationContract.SendNotice(wodt.HandlerID, "工单状态更新提醒", msg, sendNotificationAction);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取用户申请的所有工单信息
        public ActionResult GetWorkOrdersByAdminId(int adminId, int pageIndex, int pageSize, int status = -99)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (adminId <= 0)
            {
                opera.Message = "请重新登陆";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            if (status == -99)
            {
                int count = _workOrderContract.Entities.Count(w => w.ApplicantId == adminId && !w.IsDeleted && w.IsEnabled);
                var entities = _workOrderContract.Entities.Where(w => w.ApplicantId == adminId && !w.IsDeleted && w.IsEnabled).OrderByDescending(w => w.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(w =>
                            new
                            {
                                w.Id,
                                w.WorkOrderTitle,
                                w.DepartmentId,
                                DepartmentName = w.Department != null ? w.Department.DepartmentName : "",
                                w.WorkOrderCategoryId,
                                WorkOrderCategoryName = w.WorkOrderCategory != null ? w.WorkOrderCategory.WorkOrderCategoryName : "",
                                w.Content,
                                CreatedTime = w.CreatedTime.ToString("yyyy年MM月dd日 hh:ss"),
                                DealtTime = w.DealtTime != null ? Convert.ToDateTime(w.DealtTime).ToString("yyyy年MM月dd日 hh:ss") : "",
                                FinishTime = w.FinishTime != null ? Convert.ToDateTime(w.FinishTime).ToString("yyyy年MM月dd日 hh:ss") : "",
                                w.HandlerID,
                                HandlerName = w.Handler != null && w.Handler.Member != null ? w.Handler.Member.RealName : "",
                                w.ImgAddress,
                                w.Status
                            }).ToList();
                opera.Data = entities;
                opera.Other = count;
            }
            else
            {
                int count = _workOrderContract.Entities.Count(w => w.ApplicantId == adminId && w.Status == status && !w.IsDeleted && w.IsEnabled);
                var entities = _workOrderContract.Entities.Where(w => w.ApplicantId == adminId && w.Status == status && !w.IsDeleted && w.IsEnabled).OrderByDescending(w => w.CreatedTime).Skip((pageIndex - 1) * pageSize).ToList().Take(pageSize).Select(w =>
                               new
                               {
                                   w.Id,
                                   w.WorkOrderTitle,
                                   w.DepartmentId,
                                   DepartmentName = w.Department != null ? w.Department.DepartmentName : "",
                                   w.WorkOrderCategoryId,
                                   WorkOrderCategoryName = w.WorkOrderCategory != null ? w.WorkOrderCategory.WorkOrderCategoryName : "",
                                   w.Content,
                                   CreatedTime = w.CreatedTime.ToString("yyyy年MM月dd日 hh:ss"),
                                   DealtTime = w.DealtTime != null ? Convert.ToDateTime(w.DealtTime).ToString("yyyy年MM月dd日 hh:ss") : "",
                                   FinishTime = w.FinishTime != null ? Convert.ToDateTime(w.FinishTime).ToString("yyyy年MM月dd日 hh:ss") : "",
                                   w.HandlerID,
                                   HandlerName = w.Handler != null && w.Handler.Member != null ? w.Handler.Member.RealName : "",
                                   w.ImgAddress,
                                   w.Status
                               }).ToList();
                opera.Data = entities;
                opera.Other = count;
            }
            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }

        #region 获取用户单个工单信息
        public ActionResult GetWorkOrdersById(int adminId, int workOrderId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (adminId <= 0)
            {
                opera.Message = "请重新登陆";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            var entities = _workOrderContract.Entities.Where(w => w.Id == workOrderId && !w.IsDeleted && w.IsEnabled).ToList().Select(w =>
                      new
                      {
                          w.Id,
                          w.WorkOrderTitle,
                          w.DepartmentId,
                          DepartmentName = w.Department != null ? w.Department.DepartmentName : "",
                          w.WorkOrderCategoryId,
                          WorkOrderCategoryName = w.WorkOrderCategory != null ? w.WorkOrderCategory.WorkOrderCategoryName : "",
                          w.Content,
                          CreatedTime = w.CreatedTime.ToString("yyyy-MM-dd hh:mm"),
                          DealtTime = w.DealtTime != null ? Convert.ToDateTime(w.DealtTime).ToString("yyyy-MM-dd hh:mm") : "",
                          FinishTime = w.FinishTime != null ? Convert.ToDateTime(w.FinishTime).ToString("yyyy-MM-dd hh:mm") : "",
                          w.HandlerID,
                          HandlerName = w.Handler != null && w.Handler.Member != null ? w.Handler.Member.RealName : "",
                          w.ImgAddress,
                          w.Status
                      }).ToList();
            opera.Data = entities;
            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion
        #endregion

        private void AddCookies(IDictionary<string, string> dics)
        {
            foreach (var dic in dics)
            {
                if (Request.Cookies[dic.Key] != null)
                {
                    Request.Cookies[dic.Key].Value = dic.Value;
                    continue;
                }
                HttpCookie aCookie = new HttpCookie(dic.Key);
                aCookie.Value = dic.Value;
                aCookie.Expires = DateTime.Now.AddMinutes(15);
                Response.Cookies.Add(aCookie);
            }
        }

        private void RemoveCookies(IDictionary<string, string> dics)
        {
            foreach (var dic in dics)
            {
                if (Request.Cookies[dic.Key] != null)
                {
                    Request.Cookies.Remove(dic.Key);
                }
            }
        }

        private OperationResult UploadImg(HttpFileCollectionBase files)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (files.Count == 0)
            {
                oper.Message = "请选择图片";
                return oper;
            }
            var fileExt = Path.GetExtension(files[0].FileName).ToLower();
            if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(("gif,jpg,jpeg,png,bmp").Split(','), fileExt.Substring(1).ToLower()) == -1)
            {

            }
            string conPath = "/Content/UploadFiles/WorkOrders/" + DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) + "/";

            conPath = conPath + DateTime.Now.ToString("HHmmssffff") + ".jpg";
            string savePath = FileHelper.UrlToPath(conPath);
            files[0].SaveAs(savePath);
            var data = conPath;
            oper = new OperationResult(OperationResultType.Success, "上传成功！", data);

            return oper;
        }

        #region 是否可以申请工单
        /// <summary>
        /// 是否可以申请工单
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult IsApplyWorkOrder()
        {
            var admin = _administratorContract.View(AuthorityHelper.OperatorId ?? 0);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "请重新登录"), JsonRequestBehavior.AllowGet);
            }
            if (_workOrderContract.Entities.Count(w => w.DepartmentId == admin.DepartmentId && w.Status != -1 && w.Status != 3) > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该部门还有已申请工单未被处理，暂无法申请新的工单"), JsonRequestBehavior.AllowGet);
            }
            return Json(new OperationResult(OperationResultType.Success), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}