using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models;
using System.Web.Routing;
using Whiskey.Utility.Helper;
using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LayoutAttribute : ActionFilterAttribute
    {
        public readonly string weburl = ConfigurationHelper.WebUrl;
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public IMemberContract _memberContract { get; set; }
        public ITemplateThemeContract _templateThemeContract { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //if (AuthorityMemberHelper.OperatorId == null)
            //    filterContext.Result = new RedirectToRouteResult("Default", new RouteValueDictionary(new { action = "Index", controller = "Home" }));
        }
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var viewResult = filterContext.Result as ViewResult;
            if (viewResult.IsNotNull())
            {
                var request = filterContext.HttpContext.Request;

                var ViewBag = viewResult.ViewBag;

                var isAjaxView = request.IsAjaxRequest();

                ViewBag.isAjaxView = isAjaxView;

                ViewBag._p_flag = request["_p_flag"] ?? "0";//请求来源0=nav，1=menu

                #region 判断用户是否登录

                ViewBag.isLogin = AuthorityMemberHelper.IsVerified;

                if (AuthorityMemberHelper.IsVerified)
                {
                    var modMember = _memberContract.Members.FirstOrDefault(f => f.Id == AuthorityMemberHelper.OperatorId);
                    if (modMember.IsNotNull())
                    {
                        ViewBag.MemberId = modMember.Id;
                        ViewBag.MemberName = modMember.MemberName;
                        ViewBag.MemberPhoto = modMember.UserPhoto.IsNotNullAndEmpty() ? weburl + modMember.UserPhoto : string.Empty;
                    }
                }

                #endregion

                #region 判断是否启用新的主题

                ViewBag.themeExist = false;

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
                var action = context.RouteData.Values["action"].ToString().ToLower();
                pageUrl = string.Format("{0}/{1}/{2}", area, controller, action);
            }
            return pageUrl;
        }

    }
}