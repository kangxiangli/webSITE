using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.ZeroStore.MobileApi.Extensions.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
{
    public class MyWorkOrderController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MyWorkOrderController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IWorkOrderContract _workOrderContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IWorkOrderCategoryContract _workOrderCategoryContract;

        protected readonly IPermissionContract _permissionContract;

        protected readonly IWorkOrderDealtWithContract _workOrderDealtWithContract;

        public MyWorkOrderController(
            IWorkOrderContract workOrderContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IWorkOrderCategoryContract workOrderCategoryContract,
            IPermissionContract permissionContract
            )
        {
            this._workOrderContract = workOrderContract;
            this._administratorContract = administratorContract;
            this._departmentContract = departmentContract;
            this._workOrderCategoryContract = workOrderCategoryContract;
            this._permissionContract = permissionContract;
        }
        #endregion

        #region 是否可以申请工单
        /// <summary>
        /// 是否可以申请工单
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult IsApplyWorkOrder(int adminId)
        {
            var admin = _administratorContract.View(adminId);
            if (admin == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"), JsonRequestBehavior.AllowGet);
            }
            if (_workOrderContract.Entities.Count(w => w.DepartmentId == admin.DepartmentId && w.Status != -1 && w.Status != 3) > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "该部门还有已申请工单未被处理，暂无法申请新的工单"), JsonRequestBehavior.AllowGet);
            }
            return Json(new OperationResult(OperationResultType.Success), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取工单类型
        /// <summary>
        /// 获取工单类型
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
        #endregion

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

        #region 图片上传
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
        #endregion
    }
}