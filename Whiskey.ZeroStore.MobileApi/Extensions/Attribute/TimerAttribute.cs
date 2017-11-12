using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Newtonsoft.Json;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Diagnostics;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Attribute
{
    public class TimerAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //在Action执行之后执行 输出到输出流中文字：After Action execute xxx   

            GetTimer(filterContext, "action").Stop();

            base.OnActionExecuted(filterContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //在Action执行前执行  
            GetTimer(filterContext, "action").Start();

            base.OnActionExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //在Result执行之后  
            var renderTimer = GetTimer(filterContext, "render");
            renderTimer.Stop();

            var actionTimer = GetTimer(filterContext, "action");
            var response = filterContext.HttpContext.Response;

            if (response.ContentType == "text/html")
            {
                //var area = filterContext.RouteData.DataTokens.ContainsKey("area") ? filterContext.RouteData.DataTokens["area"].ToString().ToLower() : string.Empty;
                //response.Write(
                //    String.Format(
                //        "<p>'{0}/{1}/{2}', Execute: {3}ms, Render: {4}ms.</p>",
                //        area,
                //        filterContext.RouteData.Values["controller"],
                //        filterContext.RouteData.Values["action"],
                //        actionTimer.ElapsedMilliseconds,
                //        renderTimer.ElapsedMilliseconds
                //    )
                //);
            }

            base.OnResultExecuted(filterContext);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            GetTimer(filterContext, "render").Start();

            base.OnResultExecuting(filterContext);
        }

        private Stopwatch GetTimer(ControllerContext context, string name)
        {
            string key = "__timer__" + name;
            if (context.HttpContext.Items.Contains(key))
            {
                return (Stopwatch)context.HttpContext.Items[key];
            }

            var result = new Stopwatch();
            context.HttpContext.Items[key] = result;
            return result;
        }
    }   
}