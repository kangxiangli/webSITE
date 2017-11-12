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
using System.IO;
using System.Web;
using Antlr3.ST;
using Whiskey.Web.Helper;
using Antlr3.ST.Language;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class WorkOrderCategoryController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkOrderCategoryController));

        protected readonly IWorkOrderCategoryContract _workOrderCategoryContract;

        protected readonly IAdministratorContract _administratorContract;

        public WorkOrderCategoryController(
            IWorkOrderCategoryContract workOrderCategoryContract,
            IAdministratorContract administratorContract
            )
        {
            this._workOrderCategoryContract = workOrderCategoryContract;
            this._administratorContract = administratorContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public JsonResult Create(WorkOrderCategoryDto dto)
        {
            dto.Notes = dto.Notes ?? "";
            var result = _workOrderCategoryContract.Insert(dto);
            return Json(result);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(WorkOrderCategoryDto dto)
        {
            var result = _workOrderCategoryContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _workOrderCategoryContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _workOrderCategoryContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<WorkOrderCategory, bool>> predicate = FilterHelper.GetExpression<WorkOrderCategory>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                var list = (from s in _workOrderCategoryContract.Entities.Where<WorkOrderCategory, int>(predicate, request.PageCondition, out count)
                            select new
                            {
                                s.Id,
                                s.IsDeleted,
                                s.IsEnabled,
                                s.CreatedTime,
                                s.WorkOrderCategoryName,
                                s.OperatorId,
                                OperatorName = s.Operator != null && s.Operator.Member != null ? s.Operator.Member.RealName : ""
                            }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public JsonResult Remove(int[] Id)
        {
            var result = _workOrderCategoryContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public JsonResult Recovery(int[] Id)
        {
            var result = _workOrderCategoryContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public JsonResult Enable(int[] Id)
        {
            var result = _workOrderCategoryContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public JsonResult Disable(int[] Id)
        {
            var result = _workOrderCategoryContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _workOrderCategoryContract.Entities.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}

