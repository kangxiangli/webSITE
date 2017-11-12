using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas
{
    /// <summary>
    /// 注册路由信息
    /// </summary>
    public class TemplatesAreaRegistration : AreaRegistration
    {
        /// <summary>
        /// 重写MVC域名
        /// </summary>
        public override string AreaName
        {
            get { return "Templates"; }
        }
        /// <summary>
        /// 重写-注册MVC域名
        /// </summary>
        /// <param name="context">注册信息上下文</param>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Templates_default",
                "Templates/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}