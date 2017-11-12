using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class StoreIdUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public StoreIdUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        //[Fact]
        public void TestGetAllStore()
        {
            var service = _fixture.Container.Resolve<IStoreContract>();
            var count = service.Stores.Count();
            var st = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                var stores = service.QueryAllStore();
            }
            st.Stop();
        }

       // [Fact]
        public void TestGetManageStore()
        {
            var storeService = _fixture.Container.Resolve<IStoreContract>();
            var adminService = _fixture.Container.Resolve<IAdministratorContract>();
            var adminIds = adminService.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => a.Id).ToList();
            var st = Stopwatch.StartNew();
            foreach (var adminId in adminIds)
            {
                var stores = storeService.QueryManageStoreId(adminId);
            }
            st.Stop();
        }

        //[Fact]
        public void TestGetAllStorage()
        {
            var storeService = _fixture.Container.Resolve<IStoreContract>();
            var storageService = _fixture.Container.Resolve<IStorageContract>();
            var adminService = _fixture.Container.Resolve<IAdministratorContract>();
            var stores = storeService.QueryAllStore();
            var storages = storeService.QueryAllStorage();
            var closeStore = storeService.GetStoresInChecking();
            var adminIds = adminService.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => a.Id).ToList();
            var st = Stopwatch.StartNew();
            foreach (var adminId in adminIds)
            {
                var manageStoreIds = storeService.QueryManageStoreId(adminId);
                var manageStorageIds = storeService.QueryManageStorageId(adminId);
                var manageStores = storeService.QueryManageStore(adminId);
                var manageStorages = storeService.QueryManageStorage(adminId);
            }
            st.Stop();
        }

        [Fact]
        public void InterSect()
        {
            var seq1 = new List<int> { 1, 2, 3 };
            var seq2 = new List<int> { 1, 2, 4 };
            var intersect = seq1.Except(seq2);
        }







    }
}