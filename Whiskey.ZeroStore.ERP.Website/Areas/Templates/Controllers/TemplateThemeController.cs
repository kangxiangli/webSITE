using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    public class TemplateThemeController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateThemeController));
        protected readonly ITemplateThemeContract _templateThemeContract;
        public TemplateThemeController(
            ITemplateThemeContract _templateThemeContract
            )
        {
            this._templateThemeContract = _templateThemeContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<TemplateTheme, bool>> predicate = FilterHelper.GetExpression<TemplateTheme>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                IQueryable<TemplateTheme> listTemplate = _templateThemeContract.templateThemes;
                var list = listTemplate.Where(predicate).Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Notes,
                    m.IsDefault,
                    m.ThemeLogo,
                    m.BackgroundImg,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.UpdatedTime,
                    ThemeFlag = m.ThemeFlag.ToString(),
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                }).ToList();
                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            return PartialView();
        }

        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(TemplateThemeDto dto)
        {
            var res = _templateThemeContract.Insert(dto);
            return Json(res);
        }
        public ActionResult Update(int Id)
        {
            var result = _templateThemeContract.Edit(Id);
            return PartialView(result);
        }
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(TemplateThemeDto dto)
        {
            var res = _templateThemeContract.Update(dto);
            return Json(res);
        }
        public ActionResult View(int Id)
        {
            var result = _templateThemeContract.templateThemes.Where(x => x.Id == Id).FirstOrDefault();
            return PartialView(result);
        }

        /// <summary>
        /// 检测模板名称是否已经存在
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckTemplateName(string themeName, int? Id)
        {
            var result = _templateThemeContract.CheckTemplateName(themeName, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _templateThemeContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _templateThemeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 设置默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>True表示已存在</returns>
        public JsonResult SetDefault(int Id)
        {
            var result = _templateThemeContract.SetDefault(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 校验模板
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateTemplate()
        {
            return PartialView();
        }

        /// <summary>
        /// 检测模板的有效性
        /// </summary>
        /// <param name="themeContent"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult ValidateTemplateContent(string themeContent)
        {
            var model = CacheAccess.GetCurrentUserMenuModule(EntityContract._adminContract, EntityContract._permissionContract, EntityContract._moduleContract);
            var data = new { adminId = 1, adminName = "系统", adminImg = "/content/images/common/avatars/5.jpg", themeExist = true, Menu = model, BreadCrumb = model.FirstOrDefault() };
            var result = ThemeRazor.TryCompile(themeContent, data);
            return Json(result);
        }
    }
}