using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
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
    }
}