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
    public class ShoppingCartUnitTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;

        public ShoppingCartUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        //[Fact]
        public void TestStat()
        {

            var productOriginNumberService = _fixture.Container.Resolve<IProductOrigNumberContract>();
            var memberService = _fixture.Container.Resolve<IMemberContract>();
            var productService = _fixture.Container.Resolve<IProductContract>();
            var shoppingCartService = _fixture.Container.Resolve<IShoppingCartItemContract>();
            var numbers = new List<string>
            {
                "BH00DA2D2SP",
"BH00DA2D20M",
"BH00DA2D20L",
"BH00DA2ISSP",
"BH00DA2IS0M",
"BH00DA2IS0L",
"BH00DACISSP",
"BH00DACIS0M",
"BH00DACIS0L",
"BH00DACF3SP",
"BH00DACF30M",
"BH00DACF30L",
"BH00BB6D2SP",
"BH00BB6D20M",
"BH00BB6D20L",
"BH00DBAF3SP",
"BH00DBAF30M",
"BH00DBAF30L",
"BH00EBAHHSP",
"BH00EBAHH0M",
"BH00EBAHH0L",
"BH00EBAF3SP",
"BH00EBAF30M",
"BH00EBAF30L",
"BH00CB6HHSP",
"BH00CB6HH0M",
"BH00CB6HH0L",
"BH00EACISSP",
"BH00EACIS0M",
"BH00EACIS0L",
            };
            var products = productService.Products.Where(p => numbers.Contains(p.ProductNumber)).ToList();

            var members = memberService.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == 5275).Take(20).Select(m => m.Id).ToList();

            foreach (var member in members)
            {
                var shoppinItems = products.Select(p => new ShoppingCartItem() { MemberId = member, ProductId = p.Id, ProductNumber = p.ProductNumber, Quantity = 10 }).ToArray();
                shoppingCartService.Insert(shoppinItems);

            }

            var allData = shoppingCartService.Entities.ToList();
        }


        [Fact]
        public void TestGetAllItem()
        {
            var shoppingCartService = _fixture.Container.Resolve<IShoppingCartItemContract>();
            var memberId = 5275;

            var items = shoppingCartService.GetItems(memberId);

        }

    }
}