using System;
using System.Collections.Generic;
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
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.App_Start;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Threading;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website
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
            EntityContract.InitContract();
            CacheDependencyConfig.SetDependency();
            if (!ConfigurationHelper.IsDevelopment())
            {
                Atten();
                SendOrderFoodSms();
            }

        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            string strUrl = Context.Request.FilePath;
            if (strUrl == "/")
            {
                Context.RewritePath("/home/index.html");
            }
            else
            {
                bool res = RouteHelper.CheckRoute(strUrl);

                if (res == true)
                {
                    Response.Redirect("/Content/Error/404.html");
                }
            }
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
                new string[] { "Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers", "Whiskey.ZeroStore.ERP.Website.Controllers" });
            route.DataTokens["area"] = "Authorities";


            //var route = routes.MapRoute(
            //    "Default",
            //    "sign/{action}.html",
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new[] { "aWeb.Controllers" });         
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

        #region 取出注册会员数据
        private static void GetQueueMembers()
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                while (true)
                {
                    if (QueueAttribute.queueMembers.Count() > 0)
                    {
                        Dictionary<MemberDto, MemberFigure> dic = QueueAttribute.queueMembers.Dequeue();
                        if (dic != null)
                        {

                        }
                    }
                }
            });
        }
        #endregion

        #region 考勤统计
        private static void Atten()
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                DateTime time = QueueAttribute.StartDate;
                int index = 0;
                while (true)
                {
                    DateTime currentTime = DateTime.Now;
                    DateTime tempTime = time.AddMinutes(2);
                    if (currentTime.Hour == time.Hour && currentTime.Minute == time.Minute && index == 0)
                    {
                        QueueAttribute.Atten();
                        index = 1;
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }
                    if (tempTime.Hour == currentTime.Hour && tempTime.Minute == currentTime.Minute && index == 1)
                    {
                        index = 0;
                    }
                }
            });
        }

        #endregion

        #region 订餐预约信息

        private static void SendOrderFoodSms()
        {
            ThreadPool.QueueUserWorkItem((x) =>
            {
                var prevTime = DateTime.Now.Date.AddDays(-1);
                while (true)
                {
                    var now = DateTime.Now;
                    if (prevTime != now.Date)
                    {
                        Utility.XmlHelper helper = new Utility.XmlHelper("OrderFood", "SendPeoConfig");
                        var xmlphones = helper.GetElement("Phones");
                        int Hour = 9; int.TryParse(helper.GetElement("SendHour")?.Value ?? "9", out Hour);
                        int Minute = 0; int.TryParse(helper.GetElement("SendMinute")?.Value ?? "0", out Minute);
                        var sendTime = now.Date.AddHours(Hour).AddMinutes(Minute);

                        if (now >= sendTime)
                        {
                            var listphones = (xmlphones?.Value ?? "").Split(new string[] { ",", "，", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(w => w.IsMobileNumber(true)).ToList();
                            if (QueueAttribute.SendOrderFoodSms(listphones))
                            {
                                prevTime = now.Date;
                            }
                            else
                            {
                                Thread.Sleep(30000);
                            }
                        }
                        else
                        {
                            Thread.Sleep(30000);
                        }
                    }
                    else
                    {
                        Thread.Sleep(now.Date.AddDays(1) - now);
                    }
                }
            });
        }

        #endregion
    }
}
