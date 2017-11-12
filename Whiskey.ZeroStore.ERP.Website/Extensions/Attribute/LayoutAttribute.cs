using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Web.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LayoutAttribute : ActionFilterAttribute
    {
        private bool isAjaxView = true;//开启View局部刷新，写到layout原因是 尽可能小的改动现有代码fsf-2016-12
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public IAdministratorContract _administratorContract { get; set; }
        public IPermissionContract _permissionContract { get; set; }
        public IModuleContract _moduleContract { get; set; }
        public ITemplateThemeContract _templateThemeContract { get; set; }

        public LayoutAttribute()
        {

        }

        /// <summary>
        /// 覆盖默认的routedata中的action名称,用于查找模块权限时,
        /// 二级页面需要使用Index模块的权限的场景
        /// </summary>
        private string _overrideActionName;
        public LayoutAttribute(string overrideActionName)
        {
            _overrideActionName = overrideActionName;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (AuthorityHelper.OperatorId == null)
                filterContext.Result = new RedirectResult("/Authorities/Login/Index", true);
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {

        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            isAjaxView = filterContext.HttpContext.Request.IsAjaxRequest() && isAjaxView;

            var viewResult = filterContext.Result as ViewResult;
            if (viewResult.IsNotNull())
            {
                var pageUrl = GetCurPageUrl(filterContext);
                var curModule = this.BreadCrumb(viewResult, pageUrl);
                viewResult.ViewBag.BreadCrumb = curModule;
                viewResult.ViewBag.isAjaxView = isAjaxView;

                #region 判断是否启用新的主题

                TemplateTheme theme = CacheAccess.GetCurTheme(_templateThemeContract, TemplateThemeFlag.ERP);
                if (theme.IsNotNull())
                {
                    viewResult.ViewBag.themePath = theme.Path;
                    var themeExist = false;
                    if (!theme.Path.IsNullOrEmpty())
                    {
                        themeExist = FileHelper.ThemeIsExist(theme.Path);
                    }
                    viewResult.ViewBag.themeExist = themeExist;
                }
                #endregion

                if (!isAjaxView)
                {
                    viewResult.ViewBag.Menu = this.Menu(viewResult, curModule);
                }
            }
        }

        /// <summary>
        /// 菜单的显示与不显示，与权限是可以独立开的模块
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public List<Module> Menu(ViewResult viewResult, Module curModule)
        {
            var model = new List<Module>();
            try
            {
                var administrator = _administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);

                //根据当前用户具有的权限，查找到用户可以访问的模块
                model = GetCurrMenu();

                if (model.IsNotNullOrEmptyThis() && administrator.IsNotNull())//&& EnvironmentHelper.IsStore
                {
                    var adminId = administrator.Id;
                    var adminName = administrator.Member.RealName;
                    var adminImg = administrator.Member.UserPhoto;
                    adminImg = adminImg.IsNullOrEmpty() ? "/content/images/common/avatars/5.jpg" : adminImg;

                    viewResult.ViewBag.AdminId = adminId;
                    viewResult.ViewBag.AdminName = adminName;
                    viewResult.ViewBag.AdminImg = adminImg;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("菜单加载出错，错误如下：" + ex.Message + "。");
            }

            return model;//.OrderBy(m => m.Sequence).ToList();
        }
        /// <summary>
        /// 保留菜单上被选中的项
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        private Module BreadCrumb(ViewResult viewResult, string pageUrl)
        {
            var breadCrumb = new Module();
            try
            {
                var module = CacheAccess.GetModules(_moduleContract).Where(w => w.PageUrl != null).FirstOrDefault(m => m.PageUrl.ToLower().Contains(pageUrl.ToLower()));

                if (module != null)
                {
                    if (module.ParentId != null)
                    {
                        var parentMenu = CacheAccess.GetModules(_moduleContract).FirstOrDefault(x => x.Id == module.ParentId);
                        var parentModel = new Module
                        {
                            ModuleName = parentMenu.ModuleName,
                            Icon = parentMenu.Icon
                        };
                        breadCrumb.Children.Add(parentModel);
                    }

                    var childrenModel = new Module
                    {
                        ModuleName = module.ModuleName,
                        Icon = module.Icon
                    };
                    breadCrumb.Children.Add(childrenModel);

                    breadCrumb.Icon = module.Icon;
                    breadCrumb.ModuleName = module.ModuleName;
                    breadCrumb.Id = module.Id;

                    viewResult.ViewBag.ModuleName = module.ModuleName;
                    viewResult.ViewBag.inval = string.Join("|", this.PageFlag(module.Id));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("面包屑加载出错，错误如下：" + ex.Message + "。");
            }
            return breadCrumb;
        }
        /// <summary>
        /// 当前页面所不具有的元素标识
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        private List<string> PageFlag(int moduleId)
        {
            List<string> listFlag = new List<string>();
            try
            {
                List<Permission> listpers = this.GetCurrPermission();
                if (listpers.IsNotNullOrEmptyThis())
                {
                    var listhasFlag = listpers.Where(w => w.ModuleId == moduleId && !string.IsNullOrWhiteSpace(w.OnlyFlag)).Select(s => s.OnlyFlag).ToList();

                    //listFlag = _pageFlag.Except(listhasFlag).ToList();
                    listFlag = this.CurrModeuleAllOnlyFlag(moduleId).Except(listhasFlag).ToList();
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
            }
            return listFlag;
        }
        /// <summary>
        /// 获取当前用户所有权限
        /// </summary>
        /// <returns></returns>
        public List<Permission> GetCurrPermission()
        {
            return CacheAccess.GetCurrentUserPermission(_administratorContract, _permissionContract);
        }
        /// <summary>
        /// 获取当前用户可以显示的菜单Module
        /// </summary>
        /// <returns></returns>
        public List<Module> GetCurrMenu()
        {
            return CacheAccess.GetCurrentUserMenuModule(_administratorContract, _permissionContract, _moduleContract);
        }
        /// <summary>
        /// 当前模块下所有的OnlyFlag，权限所能控制到的页面元素
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public List<string> CurrModeuleAllOnlyFlag(int moduleId)
        {
            return CacheAccess.GetPermissions(_permissionContract).Where(w => w.ModuleId == moduleId && !string.IsNullOrWhiteSpace(w.OnlyFlag)).Select(s => s.OnlyFlag).ToList();
        }
        /// <summary>
        /// 获取当前请求的PageUrl
        /// </summary>
        /// <param name="context"></param>
        /// <returns>返回格式{area}/{controller}/{action}</returns>
        private string GetCurPageUrl(ControllerContext context)
        {
            string pageUrl = string.Empty;
            if (context.IsNotNull())
            {
                var area = context.RouteData.DataTokens.ContainsKey("area") ? context.RouteData.DataTokens["area"].ToString().ToLower() : string.Empty;
                var controller = context.RouteData.Values["controller"].ToString().ToLower();
                var action = _overrideActionName ?? context.RouteData.Values["action"].ToString().ToLower();
                pageUrl = string.Format("{0}/{1}/{2}", area, controller, action);
            }
            return pageUrl;
        }
    }
}