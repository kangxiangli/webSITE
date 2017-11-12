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
    public class NavInfoAttribute : ActionFilterAttribute
    {
        public readonly string weburl = ConfigurationHelper.WebUrl;
        public static ILogger _Logger = LogManager.GetLogger(typeof(LayoutAttribute));
        public IMemberContract _memberContract { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
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
                //var request = filterContext.HttpContext.Request;

                var ViewBag = viewResult.ViewBag;
                ViewBag._showNavInfo = true;
            }
        }
    }
}