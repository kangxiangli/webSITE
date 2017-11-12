using Autofac;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Warehouse;

namespace GenerateInventoryRecord
{
    class Program
    {
        private static IContainer _container;
        static void Main(string[] args)
        {
            InitContainer();
            DatabaseInitialize();
            MapperConfig.MapperRegister();
            //获取分组库存信息
            var inventoryContract = _container.Resolve<IInventoryContract>();
            var inventoryRecordContract = _container.Resolve<IInventoryRecordContract>();

            var list = inventoryContract.Inventorys.Where(i => !i.InventoryRecordId.HasValue).ToList();
            var groupList = list.GroupBy(i => IgnaoreMillSecond(i.CreatedTime)).Select(g => new
            {
                g.Key,
                Count = g.Count(),
                Ids = g.Select(r => r.Id).ToList()
            }).ToList();

            //生成入库信息
          
            foreach (var item in groupList)
            {
                var inves = list.Where(i => item.Ids.Contains(i.Id)).ToList();
                if (inves.Count != item.Count)
                {
                    throw new Exception("数量不一致");
                }

                var record = new InventoryRecord()
                {
                    Quantity = inves.Count,
                    OperatorId = inves.First().OperatorId,
                    StorageId = inves.First().StorageId,
                    StoreId = inves.First().StoreId,
                    TagPrice = inves.Sum(i => i.Product.ProductOriginNumber.TagPrice),
                    RecordOrderNumber = null,
                    CreatedTime = inves.First().CreatedTime
                };
                record.Inventories = inves;
                var opt = inventoryRecordContract.Insert(record);
            }
            
            Console.WriteLine("ok");

        }

        private static DateTime IgnaoreMillSecond(DateTime dt)
        {
            var seconds = dt.Millisecond;
            var newDt = dt.Subtract(TimeSpan.FromMilliseconds(seconds));
            return newDt;
        }

        private static void DatabaseInitialize()
        {
            Assembly modelAssembly = Assembly.Load("Whiskey.ZeroStore.ERP.Models");
            DatabaseInitializer.AddMapperAssembly(modelAssembly);
            // SeedInitialize();
            DatabaseInitializer.Initialize();
        }


        private static void InitContainer()
        {
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
                .AsImplementedInterfaces().InstancePerLifetimeScope();//InstancePerLifetimeScope 保证生命周期基于请求

            _container = builder.Build();


        }
    }
}
