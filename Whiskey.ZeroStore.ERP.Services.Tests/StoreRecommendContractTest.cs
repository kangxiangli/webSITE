using System.Collections.Generic;
using System.Linq;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;

namespace UnitTest
{
    public class StoreRecommendContractTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public StoreRecommendContractTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public void TestStoreRecommends()
        {
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var query = contract.StoreRecommends;
            var count = query.Count();
            Assert.NotNull(contract);
            Assert.NotNull(query);

        }

        [Fact]
        public void TestInsert()
        {
            var entity = new StoreRecommend()
            {
                BigProdNum = "DA46GB1",
                StoreId = 14,
            };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();

            var res = contract.Insert(entity);
            Assert.True(res.ResultType == OperationResultType.Success);

        }

        [Fact]
        public void TestDisable()
        {
            var id = new int[] { 1, 2, 3 };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var res = contract.Disable(id);
            Assert.True(res.ResultType == OperationResultType.Success);
        }

        [Fact]
        public void TestEnable()
        {
            var id = new int[] { 1, 2, 3 };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var res = contract.Enable(id);
            Assert.True(res.ResultType == OperationResultType.Success);
        }

        [Fact]
        public void TestDelete()
        {
            var id = new int[] { 1, 2, 3 };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var res = contract.Delete(id);
            var entities = contract.StoreRecommends.Where(s => id.Contains(s.Id)).ToList();
            Assert.True(res.ResultType == OperationResultType.Success);
            foreach (var item in entities)
            {
                Assert.True(item.IsDeleted);
            }

        }

        [Fact]
        public void TestRecovery()
        {
            var id = new int[] { 1, 2, 3 };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var res = contract.Recovery(id);
            var entities = contract.StoreRecommends.Where(s => id.Contains(s.Id)).ToList();
            Assert.True(res.ResultType == OperationResultType.Success);
            foreach (var item in entities)
            {
                Assert.True(item.IsDeleted == false);
            }

        }

        [Fact]
        public void TestUpdate()
        {
            var ids = new int[] { 1, 2, 3 };
            var contract = _fixture.Container.Resolve<IStoreRecommendContract>();
            var list = contract.StoreRecommends.Where(i => ids.Contains(i.Id)).ToList();
            foreach (var item in list)
            {
                item.IsDeleted = false;
                item.IsEnabled = true;
            }

            var res = contract.Update(list.ToArray());
            Assert.True(res.ResultType == OperationResultType.Success);
            var list2 = contract.StoreRecommends.Where(i => ids.Contains(i.Id)).ToList();
            foreach (var item in list2)
            {
                Assert.True(item.IsDeleted == false);
                Assert.True(item.IsEnabled == true);
            }

        }



        [Fact]
        public void AddModule()
        {

            var moduleContract = _fixture.Container.Resolve<IModuleContract>();
            var permissionContract = _fixture.Container.Resolve<IPermissionContract>();



            var parentModule = moduleContract.Modules.FirstOrDefault(m => m.ModuleName == "店铺管理模块");
            var module = new ModuleDto()
            {
                ModuleName = "商城款号管理",
                PageArea = "Stores",
                PageController = "StoreRecommend",
                PageAction = "Index",
                PageUrl = "/Stores/StoreRecommend/Create",
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
        public void AddModule2()
        {

            var moduleContract = _fixture.Container.Resolve<IModuleContract>();
            var permissionContract = _fixture.Container.Resolve<IPermissionContract>();
            var parentModule = moduleContract.Modules.FirstOrDefault(m => m.ModuleName == "店铺管理模块");
            var module = new ModuleDto()
            {
                ModuleName = "商城款号列表",
                PageArea = "Stores",
                PageController = "StoreRecommend",
                PageAction = "Index",
                PageUrl = "/Stores/StoreRecommend/Index",
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
    }
}
