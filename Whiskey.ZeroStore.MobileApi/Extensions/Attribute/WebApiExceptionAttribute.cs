using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.MobileApi.Extensions.Attribute
{
    /// <summary>
    /// 自定义WEBAPI错误处理
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class WebApiExceptionAttribute : ExceptionFilterAttribute
    {
        public static ILogger _Logger = LogManager.GetLogger(typeof(WebApiExceptionAttribute));
        public override void OnException(HttpActionExecutedContext context)
        {
            ILogger log = LogManager.GetLogger(HttpContext.Current.Request.Url.LocalPath);
            log.Error(context.Exception);

            var message = context.Exception.Message;
            if (context.Exception.InnerException != null)
                message = context.Exception.InnerException.Message;

            //context.Response = new HttpResponseMessage() { Content = new StringContent(message) };
            JavaScriptSerializer js = new JavaScriptSerializer();
            //js.Deserialize("")
            context.Response = new HttpResponseMessage() { Content = new StringContent(JsonHelper.ToJson("222")) }; //new JsonResult { Data = new OperationResult(OperationResultType.Error, "访问异常！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                //new JsonResult { Data = new OperationResult(OperationResultType.Error, "访问异常！"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            base.OnException(context);
        }
    }
}