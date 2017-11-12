using Autofac;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements.Notice;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Xunit;

namespace UnitTest
{
    public class AppointmentUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public AppointmentUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestStat()
        {

            var productOriginNumberService = _fixture.Container.Resolve<IProductOrigNumberContract>();
            var memberService = _fixture.Container.Resolve<IMemberContract>();
            var productService = _fixture.Container.Resolve<IProductContract>();

            var AppointmentService = _fixture.Container.Resolve<IAppointmentContract>();
            //            var numbers = new List<string>
            //            {
            //                "BH00DA2D2SP",
            //"BH00DA2D20M",
            //"BH00DA2D20L",
            //"BH00DA2ISSP",
            //"BH00DA2IS0M",
            //"BH00DA2IS0L",
            //"BH00DACISSP",
            //"BH00DACIS0M",
            //"BH00DACIS0L",
            //"BH00DACF3SP",
            //"BH00DACF30M",
            //"BH00DACF30L",
            //"BH00BB6D2SP",
            //"BH00BB6D20M",
            //"BH00BB6D20L",
            //"BH00DBAF3SP",
            //"BH00DBAF30M",
            //"BH00DBAF30L",
            //"BH00EBAHHSP",
            //"BH00EBAHH0M",
            //"BH00EBAHH0L",
            //"BH00EBAF3SP",
            //"BH00EBAF30M",
            //"BH00EBAF30L",
            //"BH00CB6HHSP",
            //"BH00CB6HH0M",
            //"BH00CB6HH0L",
            //"BH00EACISSP",
            //"BH00EACIS0M",
            //"BH00EACIS0L",
            //            };
            var allRecommends = productOriginNumberService.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && o.IsRecommend.Value == true).Select(o => new { o.BigProdNum, o.RecommendStoreIds }).ToList();
            var allBigProdNums = allRecommends.Select(r => r.BigProdNum).Distinct().ToList();
            var allProducts = productService.Products.Where(p => !p.IsDeleted && p.IsEnabled && allBigProdNums.Contains(p.BigProdNum)).Select(p => new { p.Id, p.BigProdNum, p.ProductNumber }).ToList();
            var members = memberService.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.StoreId.HasValue && m.Store.IsAttached && m.Store.IsEnabled && !m.Store.IsDeleted).Select(m => new { m.Id, StoreId = m.StoreId }).ToList();

          

            foreach (var member in members)
            {
                var storeRecommendNumbers = allRecommends.Where(r => r.RecommendStoreIds.Contains(member.StoreId.Value.ToString())).Select(r => r.BigProdNum).ToList();

                var products = allProducts.Where(p => storeRecommendNumbers.Contains(p.BigProdNum))
                    .Select(p => new AppointmentProductEntry() {  ProductNumber = p.ProductNumber })
                    .ToArray();
                var entitiesToAdd = products.Select(d => new Appointment()
                {
                    MemberId = member.Id,
                    StoreId = member.StoreId.Value,
                    ProductNumber = d.ProductNumber,
                    State = AppointmentState.预约中
                }).ToArray();

                AppointmentService.BatchInsert(entitiesToAdd);
            }

            //var allData = AppointmentService.Entities.ToList();
        }


        [Fact]
        public void TestGetAllItem()
        {
            var AppointmentService = _fixture.Container.Resolve<IAppointmentContract>();
            var memberId = 5275;

            var items = AppointmentService.GetItems(memberId);

        }

    }
}