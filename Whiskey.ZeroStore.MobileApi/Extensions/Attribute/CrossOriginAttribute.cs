using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CrossOriginAttribute : ActionFilterAttribute
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
            reponse.Headers.Add("Access-Control-Allow-Credentials", "true");
        }
    }
}