using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core.Data.Entity;
using Whiskey.Core.Data;
using System.Reflection;
using Whiskey.Core;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Website;
using Autofac.Integration.Mvc;

namespace UnitTest
{
    public class CommonTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public CommonTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestContainerResolve()
        {
            // 获取运行程序集
            List<Assembly> assList = new List<Assembly>();
            assList.Add(Assembly.GetExecutingAssembly());
            var refAss = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load).ToList();
            assList.AddRange(refAss);

            //var path = Directory.GetCurrentDirectory();
            //var files = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            //for (int i = 0; i < files.Length; i++)
            //{
            //    var ass = Assembly.LoadFile(files[i]);
            //    assList.Add(ass);
            //}
            var svcAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Services");
            assList.Add(svcAssembly);
            // 筛选出所有的IxxContract
            var typeList = assList.SelectMany(p => p.GetTypes()).ToList();
            var baseType = typeof(IDependency);
            var allContracTypes = typeList.Where(t => t.IsInterface && t.IsAssignableTo<IDependency>() && t != baseType && t != typeof(IRepository<,>)).ToList();
            Assert.True(allContracTypes != null && allContracTypes.Count > 0);

            // 使用容器逐个创建注册实例
            for (int i = 0; i < allContracTypes.Count; i++)
            {
                var instance = _fixture.Container.Resolve(allContracTypes[i]);
                Assert.NotNull(instance);
            }



        }

        [Fact]
        public void GenerateStrNum()
        {
            var id = 5275;
            var contract = _fixture.Container.Resolve<IMemberContract>();
            var member = contract.Members.FirstOrDefault(m => m.Id == id);
            string num = (member.MobilePhone + member.UniquelyIdentifies).MD5Hash();
        }

        [Fact]
        public void AddModule()
        {

            var moduleContract = _fixture.Container.Resolve<IModuleContract>();
            var permissionContract = _fixture.Container.Resolve<IPermissionContract>();
            var module = new ModuleDto()
            {
                ModuleName = "店铺活动",
                PageArea = "Stores",
                PageController = "StoreActivity",
                PageAction = "Index",
                PageUrl = "/Stores/StoreActivity/Index",
                ModuleType = 1,
                ParentId = 11
            };
            var res = moduleContract.Insert(module);

            var entity = moduleContract.Modules.FirstOrDefault(m => m.ModuleName == module.ModuleName);
            var permissionList = new List<PermissionDto>
            {
                // index 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "加载页面",
                    Description = "加载页面",
                    Identifier = "Index",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Index",
                    Gtype = PermissionGroupType.查看
                },

                // list 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "加载数据",
                    Description = "加载数据",
                    Identifier = "List",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "List",
                    Gtype = PermissionGroupType.查看
                },

                 // View 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "查看数据详情",
                    Description = "查看数据详情",
                    Identifier = "View",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "View",
                    OnlyFlag = "#View",
                    Gtype = PermissionGroupType.查看
                },


                // Create 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "新增",
                    Description = "新增",
                    Identifier = "Create",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Create",
                    OnlyFlag = "#Create",
                    Gtype = PermissionGroupType.新增

                },
                // Update 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "修改",
                    Description = "修改",
                    Identifier = "Update",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Update",
                    OnlyFlag = "#Update",
                    Gtype = PermissionGroupType.修改
                },

                // Remove 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "移除数据",
                    Description = "移除数据",
                    Identifier = "Remove",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Remove",
                    OnlyFlag = "#Remove",
                    Gtype = PermissionGroupType.删除
                },
                // RemoveAll 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "批量删除",
                    Description = "批量删除",
                    Identifier = "RemoveAll",
                    ActionName = "RemoveAll",
                    OnlyFlag = "#RemoveAll",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.删除
                },
                
                // Recovery 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "将数据从回收站恢复",
                    Description = "将数据从回收站恢复",
                    Identifier = "Recovery",
                    ActionName = "Recovery",
                    OnlyFlag = "#Recovery",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                     Gtype = PermissionGroupType.删除
                },
                // Print 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "打印预览数据",
                    Description = "打印预览数据",
                    Identifier = "Print",
                    ActionName = "Print",
                    OnlyFlag = "#Print",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.查看
                },
                // Export 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "导出数据",
                    Description = "导出数据",
                    Identifier = "Export",
                    ActionName = "Export",
                    OnlyFlag = "#Export",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.查看
                },
                
                // Enable 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "启用",
                    Description = "启用",
                    Identifier = "Enable",
                    ActionName = "Enable",
                    OnlyFlag = "#Enable",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.禁用
                },
                // Disable 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "禁用数据",
                    Description = "禁用数据",
                    Identifier = "Disable",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Disable",
                    OnlyFlag = "#Disable",
                    Gtype = PermissionGroupType.禁用
                },
        };

            var res2 = permissionContract.Insert(permissionList.ToArray());


        }

        [Fact]
        public void AddTimeoutModule()
        {

            var moduleContract = _fixture.Container.Resolve<IModuleContract>();
            var permissionContract = _fixture.Container.Resolve<IPermissionContract>();
            var parentModule  = moduleContract.Modules.Where(m => m.ModuleName.Contains("公共管理模块")).FirstOrDefault();
            var module = new ModuleDto()
            {
                ModuleName = "超时管理",
                PageArea = "Common",
                PageController = "TimeoutConfig",
                PageAction = "Index",
                PageUrl = "/Common/TimeoutConfig/Index",
                ModuleType = 1,
                ParentId = parentModule.Id
            };
            var res = moduleContract.Insert(module);

            var entity = moduleContract.Modules.FirstOrDefault(m => m.ModuleName == module.ModuleName);
            var permissionList = new List<PermissionDto>
            {
                // index 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "加载页面",
                    Description = "加载页面",
                    Identifier = "Index",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Index",
                    Gtype = PermissionGroupType.查看
                },

                // list 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "加载数据",
                    Description = "加载数据",
                    Identifier = "List",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "List",
                    Gtype = PermissionGroupType.查看
                },

                 // View 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "查看数据详情",
                    Description = "查看数据详情",
                    Identifier = "View",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "View",
                    OnlyFlag = "#View",
                    Gtype = PermissionGroupType.查看
                },


                // Create 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "新增",
                    Description = "新增",
                    Identifier = "Create",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Create",
                    OnlyFlag = "#Create",
                    Gtype = PermissionGroupType.新增

                },
                // Update 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "修改",
                    Description = "修改",
                    Identifier = "Update",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Update",
                    OnlyFlag = "#Update",
                    Gtype = PermissionGroupType.修改
                },

                // Remove 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "移除数据",
                    Description = "移除数据",
                    Identifier = "Remove",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Remove",
                    OnlyFlag = "#Remove",
                    Gtype = PermissionGroupType.删除
                },
                // RemoveAll 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "批量删除",
                    Description = "批量删除",
                    Identifier = "RemoveAll",
                    ActionName = "RemoveAll",
                    OnlyFlag = "#RemoveAll",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.删除
                },
                
                // Recovery 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "将数据从回收站恢复",
                    Description = "将数据从回收站恢复",
                    Identifier = "Recovery",
                    ActionName = "Recovery",
                    OnlyFlag = "#Recovery",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                     Gtype = PermissionGroupType.删除
                },
                // Print 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "打印预览数据",
                    Description = "打印预览数据",
                    Identifier = "Print",
                    ActionName = "Print",
                    OnlyFlag = "#Print",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.查看
                },
                // Export 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "导出数据",
                    Description = "导出数据",
                    Identifier = "Export",
                    ActionName = "Export",
                    OnlyFlag = "#Export",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.查看
                },
                
                // Enable 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "启用",
                    Description = "启用",
                    Identifier = "Enable",
                    ActionName = "Enable",
                    OnlyFlag = "#Enable",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    Gtype = PermissionGroupType.禁用
                },
                // Disable 权限
                new PermissionDto()
                {
                    ModuleId = entity.Id,
                    PermissionName = "禁用数据",
                    Description = "禁用数据",
                    Identifier = "Disable",
                    Icon = "fa-search",
                    Style = "btn btn-primary btn-padding-right",
                    ActionName = "Disable",
                    OnlyFlag = "#Disable",
                    Gtype = PermissionGroupType.禁用
                },
        };

            var res2 = permissionContract.Insert(permissionList.ToArray());


        }



        [Fact]
        public void TestReturnNumber()
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

    }


    /// <summary>
    /// 容器服务初始化
    /// </summary>
    public class AutofacFixture : IDisposable
    {
        private static IContainer _container;

        /// <summary>
        /// 容器对象初始化
        /// </summary>
        public AutofacFixture()
        {

        }

        private void init()
        {

            MvcApplication.AutofacMvcRegister();

            var builder = new Autofac.ContainerBuilder();
            // 为所有的仓储接口注册
            builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>));

            // 为所有标记了IDependency的接口注册
            Type baseType = typeof(IDependency);
            var assemblies = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .ToList();

            assemblies.Add(Assembly.Load("Whiskey.ZeroStore.ERP.Services"));
            assemblies.Add(Assembly.GetExecutingAssembly());
            var ass = assemblies.ToArray();

            builder.RegisterAssemblyTypes(ass)
                .Where(type => baseType.IsAssignableFrom(type) && !type.IsAbstract)
                .AsImplementedInterfaces().InstancePerDependency();//InstancePerLifetimeScope 保证生命周期基于请求
            builder.RegisterControllers(ass);

            builder.RegisterFilterProvider();

            _container = builder.Build();

            //数据库映射
            DatabaseInitialize();
           

            //AutoMapper映射
            MapperConfig.MapperRegister();

        }

        public IContainer Container
        {
            get
            {
                if (_container == null)
                {
                    init();
                }
                return _container;
            }
        }
        public void Dispose()
        {



        }

        private static void DatabaseInitialize()
        {
            Assembly modelAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Models");
            DatabaseInitializer.AddMapperAssembly(modelAssembly);
            DatabaseInitializer.Initialize();
        }
    }
}
