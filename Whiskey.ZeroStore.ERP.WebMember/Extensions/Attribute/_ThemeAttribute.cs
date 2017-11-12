using System;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class _ThemeAttribute : ActionFilterAttribute
    {
        public ITemplateThemeContract _templateThemeContract { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var viewResult = filterContext.Result as ViewResult;

            if (viewResult.IsNotNull())
            {
                var request = filterContext.HttpContext.Request;
                var ViewBag = viewResult.ViewBag;
                ViewBag.themeExist = false;
                ViewBag._p_flag = request["_p_flag"] ?? "0";//请求来源0=nav，1=menu

                #region 判断是否启用新的主题

                TemplateTheme theme = CacheAccess.GetCurTheme(_templateThemeContract, TemplateThemeFlag.Mall);
                if (theme.IsNotNull())
                {
                    ViewBag.themePath = theme.Path;
                    var themeExist = false;
                    if (!theme.Path.IsNullOrEmpty())
                    {
                        themeExist = FileHelper.ThemeMallIsExist(theme.Path);
                    }
                    ViewBag.themeExist = themeExist;
                }
                #endregion

            }
        }
    }
}