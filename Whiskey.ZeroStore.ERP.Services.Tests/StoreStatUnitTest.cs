using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class StoreStatUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public StoreStatUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestStat()
        {
            var storeId = 14;
            var date = "20170420";
            var service = _fixture.Container.Resolve<IStoreStatisticsContract>();

            var model2 = service.StatData(storeId, date);
        }

        //[Fact]
        public void TestLogIn()
        {

            var statService = _fixture.Container.Resolve<IStoreStatisticsContract>();
            var adminService = _fixture.Container.Resolve<IAdministratorContract>();
            var ids = adminService.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => a.Id).ToList();
            var adminController = _fixture.Container.Resolve<AdminController>();
            const string macAddr = "D4EE07346652";
            var watch = Stopwatch.StartNew();
            foreach (var item in ids)
            {
                var res = adminController.LoginIn(item.ToString(), macAddr);
                var jsonRes = res.Data as OperationResult;
                Assert.True(jsonRes.ResultType == OperationResultType.Success);
            }
            watch.Stop();

        }

        //[Fact]
        public async void TestLogOut()
        {

            var statService = _fixture.Container.Resolve<IStoreStatisticsContract>();
            var adminService = _fixture.Container.Resolve<IAdministratorContract>();
            var ids = adminService.Administrators.Where(a => !a.IsDeleted && a.IsEnabled).Select(a => a.Id).ToList();
            var adminController = _fixture.Container.Resolve<AdminController>();
            const string macAddr = "D4EE07346652";
            TimeSpan spanTime = TimeSpan.Zero;
            foreach (var item in ids)
            {
                var watch = Stopwatch.StartNew();
                var res = await Task.Run(() => { return adminController.LoginOut(item.ToString(), macAddr); });
                var jsonRes = res.Data as OperationResult;
                Assert.True(jsonRes.ResultType == OperationResultType.Success);
                watch.Stop();
                spanTime += watch.Elapsed;
            }
            var avgspan = spanTime.TotalSeconds * 1.0 / ids.Count;

        }

        //[Fact]
        public void TestCategorySaleCount()
        {
            var service = _fixture.Container.Resolve<IStatisticsContract>();
            var adminSvc = _fixture.Container.Resolve<IAdministratorContract>();
            var data = service.GetCategorySaleStatInfo(9, 15);
        }

        [Fact]
        public void ttt()
        {
            var service = _fixture.Container.Resolve<IStoreStatisticsContract>();
            service.StatOrderCount();
        }
        
    }
}