using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //注册WEBAPI异常信息处理
            //config.Filters.Add(new WebApiExceptionAttribute());


            

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            
        }
    }
}
