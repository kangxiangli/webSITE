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
    public class LogAttribute : ActionFilterAttribute
    {
        public ILogContract _logContract { get; set; }

        public string _area;
        public string _controller;
        public string _action;
        public string _operator;

        public override void OnActionExecuting(ActionExecutingContext filterContext) {

            _area = filterContext.RouteData.DataTokens.ContainsKey("area") ? filterContext.RouteData.DataTokens["area"].ToString() : string.Empty;
            _controller = filterContext.RouteData.Values["controller"].ToString();
            _action = filterContext.RouteData.Values["action"].ToString();
            _operator = AuthorityHelper.AdminName + "（ID：" + AuthorityHelper.OperatorId + "，姓名：" + AuthorityHelper.RealName + "）";
            var formPost=HttpContext.Current.Request.Form.ToString();

            _logContract.Insert(new LogDto
            {
                LogName = _operator + "执行了" + _action + "操作",
                Description = formPost,
                PageUrl = "/" + _area + "/" + _controller + "/" + _action,
                IPAddress = EnvironmentHelper.GetIP(),
            });
        }
    }
}