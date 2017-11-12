using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Whiskey.ZeroStore.ERP.WebMember.Extensions.Web
{
    public class RouteInfo:RouteBase
    {
        #region 初始化操作对象

        protected string Url { get; set; }

        protected string Area { get; set; }

        protected string Controller { get; set; }

        protected string Action { get; set; }

        #endregion         

        #region 路由重写
        public override RouteData GetRouteData(HttpContextBase context)
        {
            string strPath = context.Request.AppRelativeCurrentExecutionFilePath + context.Request.PathInfo;
            strPath = strPath.Substring(2).Trim('/');
            if (string.IsNullOrEmpty(strPath)) return null;            
            var route = new RouteData(this, new MvcRouteHandler());

            if (strPath.Equals("admin_login", StringComparison.OrdinalIgnoreCase))
            {
                Area = "Authorities";
                Controller = "Login";
                Action = "Index";
                //Url = entity.OverrideUrl;
                route.DataTokens.Add("area", Area);
                route.Values.Add("controller", Controller);
                route.Values.Add("action", Action);
                return route;
            }
            else { return null; }
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext,RouteValueDictionary routeDic)
        {
            if (!routeDic.ContainsKey("controller") || !routeDic["controller"].ToString().Equals(Controller, StringComparison.OrdinalIgnoreCase))
                return null;
            if (!routeDic.ContainsKey("action") || !routeDic["action"].ToString().Equals(Action, StringComparison.OrdinalIgnoreCase))
                return null;            
            return new VirtualPathData(this, Url.ToLowerInvariant());
        }
        #endregion

    }
}