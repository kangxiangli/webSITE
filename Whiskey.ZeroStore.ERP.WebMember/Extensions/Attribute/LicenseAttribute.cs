using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class LicenseAttribute : AuthorizeAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        /// <summary>
        /// 返回首页的重定向路由
        /// </summary>
        private readonly ActionResult HomeActionResult = new RedirectToRouteResult("Default", new RouteValueDictionary(new { action = "Index", controller = "Home" }));

        public IMemberContract _memberContract { get; set; }

        public CheckMode _checkMode;

        public LicenseAttribute(CheckMode mode)
        {
            _checkMode = mode;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!AuthorityMemberHelper.IsVerified)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.LoginError, "登录信息已经超时！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                else
                {
                    FormsAuthentication.SignOut();
                    filterContext.Result = HomeActionResult;
                }
            }
            else
            {
                var member = _memberContract.Members.FirstOrDefault(m => m.Id == AuthorityMemberHelper.OperatorId);
                if (member == null)
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.Result = new JsonResult { Data = new OperationResult(OperationResultType.LoginError, "登录信息有误，请重新登录！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                    else
                    {
                        FormsAuthentication.SignOut();
                        filterContext.Result = HomeActionResult;
                    }
                }
                else
                {
                    if (_checkMode == CheckMode.Verify)
                    {

                    }
                }
            }

        }
    }
}
