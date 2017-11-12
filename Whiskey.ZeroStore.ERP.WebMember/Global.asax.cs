using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using Autofac;
using Autofac.Integration.Mvc;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.Core.Data.Entity.Migrations;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Web;
using Whiskey.ZeroStore.ERP.WebMember.App_Start;

namespace Whiskey.ZeroStore.ERP.WebMember
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Application["OnlineCount"] = 0;

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AreaRegistration.RegisterAllAreas();
            RoutesRegister();
            MapperConfig.MapperRegister();
            AutofacMvcRegister();
            DatabaseInitialize();
            LoggingInitialize();
            CacheDependencyConfig.SetDependency();
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            string strUrl = Context.Request.FilePath;
            if (strUrl == "/")
            {
                //Context.RewritePath("/home/index.html");
            }
            else
            {
                //bool res = RouteHelper.CheckRoute(strUrl);

                //if (res == true)
                //{
                //    Response.Redirect("/Content/Error/index.html");
                //}
            }
        }

        protected void Application_EndRequest()
        {
            //var statusCode = Context.Response.StatusCode;
            //var routingData = Context.Request.RequestContext.RouteData;
            //if (statusCode == 404)
            //{
            //    Response.Redirect("/Content/Error/index.html");
            //}
        }
        protected void Application_Error(object sender, EventArgs e)
        {

        }

        #region Session开始

        /// <summary>
        /// 每个客户端访问都会分配一个唯一的SessionId
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_Start(object sender, EventArgs e)
        {
            object obj = Application["OnlineCount"];
            Application["OnlineCount"] = Convert.ToInt32(obj) + 1;
        }
        #endregion

        #region Session结束
        /// <summary>
        /// 当用户离开网站或用户的会话终结
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Sesion_End(object sender, EventArgs e)
        {
            object obj = Application["OnlineCount"];
            Application["OnlineCount"] = Convert.ToInt32(obj) - 1;
        }
        #endregion
        private static void RoutesRegister()
        {
            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            var data = new RouteInfo();
            if (data != null)
            {
                routes.Add(data);
            }

            var route = routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Whiskey.ZeroStore.ERP.WebMember.Controllers" });
        }

        public static void AutofacMvcRegister()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>));
            Type baseType = typeof(IDependency);

            Assembly[] assemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                .Select(Assembly.Load).ToArray();
            assemblies = assemblies.Union(new[] { Assembly.GetExecutingAssembly() }).ToArray();
            builder.RegisterAssemblyTypes(assemblies)
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
                .AsImplementedInterfaces().InstancePerLifetimeScope();//InstancePerLifetimeScope 保证生命周期基于请求

            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            ContractHelper.SetContainer(container);
        }

        private static void DatabaseInitialize()
        {
            Assembly modelAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Models");
            DatabaseInitializer.AddMapperAssembly(modelAssembly);
            // SeedInitialize();
            DatabaseInitializer.Initialize();
        }
        private static void SeedInitialize()
        {
            Assembly seedAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Models");
            Type[] types = seedAssembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("ISeedAction") != null && !type.IsAbstract)
                {
                    ISeedAction seed = (ISeedAction)seedAssembly.CreateInstance(type.FullName);
                    CreateDatabaseIfNotExistsWithSeed.SeedActions.Add(seed);
                }
            }
        }

        private static void LoggingInitialize()
        {
            Log4NetLoggerAdapter adapter = new Log4NetLoggerAdapter();
            LogManager.AddLoggerAdapter(adapter);
        }

    }
}
