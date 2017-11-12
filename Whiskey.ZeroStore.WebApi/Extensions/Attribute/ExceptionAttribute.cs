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

namespace Whiskey.ZeroStore.WebApi.Extensions.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ExceptionAttribute : FilterAttribute, IExceptionFilter  
    {

        public static ILogger _Logger = LogManager.GetLogger(typeof(ExceptionAttribute));

        public void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception;

            _Logger.Error(ex.ToString());

            // 1.记录异常日志  
            // TODO  

            // 2.实施补救措施  
            // TODO  

            // 3.展示错误页面：默认为~/Views/Shared/Error.cshtml，由HandleErrorAttribute捕获  
            if (ex is NullReferenceException)
            {
                //filterContext.Result = new ViewResult { ViewName = MVC.Home.Views.Index };
            }
            else if (ex is HttpAntiForgeryException)
            {
                //filterContext.Result = new RedirectResult("/Authentication/LogOn");
            }
            // ...  
        }  
    }
}