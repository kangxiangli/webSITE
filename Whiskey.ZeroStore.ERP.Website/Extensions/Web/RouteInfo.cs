using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Extensions.Web
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
            List<Module> listModule = RouteHelper.GetModuleList();
            Module entity = listModule.FirstOrDefault(x => strPath.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase));
            if (entity==null)
            {
                int length = strPath.Split('/').Length;
                if (length==2)
                {
                    strPath = "/"+strPath + "/index";                    
                }
                entity = listModule.FirstOrDefault(x => x.PageUrl.ToLowerInvariant()==strPath.ToLowerInvariant());
                if (entity == null)
                {
                    return null;
                }
                else
                {
                    if (string.IsNullOrEmpty(entity.OverrideUrl))
                    {
                        Area = entity.PageArea;
                        Controller = entity.PageController;
                        Action = entity.PageAction;
                        Url = entity.OverrideUrl;
                        route.DataTokens.Add("area", Area);
                        route.Values.Add("controller", Controller);
                        route.Values.Add("action", Action);
                        return route;                        
                    }
                    else
                    {
                        return null;
                    }                    
                }
            }
            else
            {
                Area= entity.PageArea;
                Controller = entity.PageController;
                Action = entity.PageAction;
                Url = entity.OverrideUrl;
                route.DataTokens.Add("area", Area);                
                route.Values.Add("controller", Controller);
                route.Values.Add("action", Action);
                return route;
            }
            
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext,RouteValueDictionary routeDic)
        {
            //if (!routeDic.ContainsKey("Members") || !routeDic["Members"].ToString().Equals("Members", StringComparison.OrdinalIgnoreCase))
            //    return null;
            if (!routeDic.ContainsKey("controller") || !routeDic["controller"].ToString().Equals(Controller, StringComparison.OrdinalIgnoreCase))
                return null;
            if (!routeDic.ContainsKey("action") || !routeDic["action"].ToString().Equals(Action, StringComparison.OrdinalIgnoreCase))
                return null;            
            return new VirtualPathData(this, Url.ToLowerInvariant());
        }
        #endregion

    }
}