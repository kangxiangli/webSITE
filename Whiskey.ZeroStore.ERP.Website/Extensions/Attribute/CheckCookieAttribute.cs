using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Attribute
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CheckCookieAttrbute : AuthorizeAttribute
    {

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AuthValidAttribute));

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return AuthorityHelper.OperatorId.HasValue;
        }
    }
   
}