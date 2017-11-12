using System;
using System.Web.Mvc;

namespace Whiskey.Web.Extensions
{
    /// <summary>
    /// 允许跨域特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowCrossAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var reponse = filterContext.HttpContext.Response;
            //var request = filterContext.HttpContext.Request;
            var allowcross = reponse.Headers.Get("Access-Control-Allow-Origin");
            if (reponse == null || !string.IsNullOrEmpty(allowcross))
                return;
            reponse.Headers.Add("Access-Control-Allow-Origin", "*");
            reponse.Headers.Add("Access-Control-Allow-Headers", "*");
            reponse.Headers.Add("Access-Control-Allow-Credentials", "true");
            reponse.Headers.Add("Access-Control-Allow-Method", "POST, GET, OPTIONS");
        }
    }
}