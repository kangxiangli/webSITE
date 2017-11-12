﻿using Autofac;
using Autofac.Integration.WebApi;
using Autofac.Integration.Mvc;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.Core.Data.Entity.Migrations;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Extensions.Web;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.MobileApi.App_Start;

namespace Whiskey.ZeroStore.MobileApi
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        //protected void Application_Start()
        //{
        //    AreaRegistration.RegisterAllAreas();

        //    WebApiConfig.Register(GlobalConfiguration.Configuration);
        //    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //}

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            AreaRegistration.RegisterAllAreas();
            RoutesRegister();
            MapperConfig.MapperRegister();            
            AutofacMvcRegister();
            DatabaseInitialize();
            LoggingInitialize();

            if (!ConfigurationHelper.IsDevelopment())
            {
                CacheDependencyConfig.SetDependency();
            }
        }

        private static void RoutesRegister()
        {
            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            var route = routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new string[] { "Whiskey.ZeroStore.MobileApi.Controllers" });
            //route.DataTokens["area"] = "Authorities";
        }

        private static void AutofacMvcRegister()
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
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            var webapiresolver = new AutofacWebApiDependencyResolver(container); ;
            GlobalConfiguration.Configuration.DependencyResolver = webapiresolver;//ApiController　WebApi注入
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void DatabaseInitialize()
        {
            Assembly modelAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Models");
            DatabaseInitializer.AddMapperAssembly(modelAssembly);
            SeedInitialize();
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

         

        protected void Application_Error(object sender, EventArgs e)
        {
            
        }

    }
}