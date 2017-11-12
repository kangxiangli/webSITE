
//  <copyright file="NonRequireHttpsAttribute.cs" company="优维拉软件设计工作室">



//  <last-date>2014-11-13 21:39</last-date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace Whiskey.Web.Mvc.Filters
{
    /// <summary>
    /// 将套字连接HTTPS转换为普通HTTP连接
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NonRequireHttpsAttribute : FilterAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (!filterContext.HttpContext.Request.IsSecureConnection)
            {
                return;
            }
            HandleHttpsRequest(filterContext);
        }

        private void HandleHttpsRequest(AuthorizationContext filterContext)
        {
            if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("HttpMethod必须为GET");
            }
            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request.Url != null)
            {
                filterContext.Result = new RedirectResult("http://" + request.Url.Host + request.RawUrl);
            }
        }
    }
}