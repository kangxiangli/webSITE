using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Logging;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using System.Linq.Expressions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.Core.Data.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    //[License(CheckMode.Verify)]
    public class NotificationTemplateController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(NotificationTemplateController));
        protected readonly ITemplateContract _templateContract;
        protected readonly ITemplateNotificationContract _templateNotificationContract;
        public NotificationTemplateController(
            ITemplateContract templateContract
            , ITemplateNotificationContract _templateNotificationContract
            )
        {
            _templateContract = templateContract;
            this._templateNotificationContract = _templateNotificationContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Template, bool>> predicate = FilterHelper.GetExpression<Template>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                IQueryable<Template> listTemplate = _templateContract.Templates.Where(m => m.TemplateType == (int)TemplateFlag.Notification);
                var count = 0;
                var list = listTemplate.Where<Template, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.TemplateName,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.IsDefault,
                    m.UpdatedTime,
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                    TemplateCate = m.templateNotification != null ? m.templateNotification.Name : string.Empty,
                    m.EnabledPerNotifi,
                    m.DepartTypeFlags,
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            var list = _templateNotificationContract.templateNotifications.Select(s => new SelectListItem() { Value = s.Id.ToString(), Text = s.Name }).ToList();
            ViewBag.noticate = list;
            return PartialView();
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Notification;
            var res = _templateContract.Insert(dto);
            return Json(res);

        }

        public ActionResult Update(int Id)
        {
            var result = _templateContract.Edit(Id);

            return PartialView(result);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Notification;
            dto.templateNotification = _templateContract.View(dto.Id).templateNotification;
            var res = _templateContract.Update(dto);
            return Json(res);
        }

        public ActionResult View(int Id)
        {
            var result = _templateContract.Templates.Where(x => x.Id == Id).FirstOrDefault();
            ViewBag.DepartTypeFlags = string.Empty;
            if (result.DepartTypeFlags.IsNotNullAndEmpty())
            {
                ViewBag.DepartTypeFlags = string.Join(",", result.DepartTypeFlags.Split(",").ToList().ConvertAll(c => (DepartmentTypeFlag)c.CastTo<int>()).Select(s => s + ""));
            }

            return PartialView(result);
        }

        /// <summary>
        /// 检测模板名称是否已经存在
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckTemplateName()
        {
            string templateName = Request["templateName"];
            int result = 0;//0表示接受的模版名称为空，1表示模版名称不存在，2表示模版名称已存在
            if (string.IsNullOrEmpty(templateName))
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = _templateContract.CheckTemplateName(templateName, TemplateFlag.Notification);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _templateContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _templateContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotes(int Id)
        {
            return Json(_templateNotificationContract.View(Id).Notes);
        }

        #region 设为默认模板
        /// <summary>
        /// 设置默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult SetDefault(int Id)
        {
            var result = _templateContract.SetDefault(Id, TemplateFlag.Notification);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
	}
}