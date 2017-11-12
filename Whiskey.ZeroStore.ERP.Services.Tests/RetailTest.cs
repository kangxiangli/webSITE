using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Services.Implements;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace UnitTest
{
    public class RetalTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public RetalTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }




        [Fact]
        public void TestCheckBarcode1()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var validbarcode = inventoryContract.Inventorys.FirstOrDefault(i => i.Status == InventoryStatus.Default && i.StoreId == 15).ProductBarcode;
            var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, validbarcode);
            Assert.True(res[validbarcode].Item1);

            var shorCode = validbarcode.Substring(0, validbarcode.Length - 1);
            res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, shorCode);
            Assert.False(res[shorCode].Item1);

            var longCode = validbarcode + "0";
            res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, longCode);
            Assert.False(res[longCode].Item1);

            var withSpaceCode = " " + validbarcode + "  ";
            res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, withSpaceCode);
            Assert.False(res[withSpaceCode].Item1);


            var lowerCode = validbarcode.ToLower();

            res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, lowerCode);
            Assert.False(res[lowerCode].Item1);
        }


        [Fact]
        public void TestCheckBarcode_Deleted()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var code = inventoryContract.Inventorys.FirstOrDefault(i => i.IsDeleted && i.StoreId == 15)?.ProductBarcode;
            if (code != null)
            {
                var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, code);
                Assert.False(res[code].Item1);
                Assert.True(res[code].Item2.Length > 0);
            }

        }


        [Fact]
        public void TestCheckBarcode_Disabled()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var code = inventoryContract.Inventorys.FirstOrDefault(i => !i.IsEnabled && i.StoreId == 15)?.ProductBarcode;
            if (code != null)
            {
                var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, code);
                Assert.False(res[code].Item1);
                Assert.True(res[code].Item2.Length > 0);
            }

        }

        [Fact]
        public void TestCheckBarcode_DeliveryLock()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var code = inventoryContract.Inventorys.FirstOrDefault(i => i.Status == InventoryStatus.DeliveryLock && i.StoreId == 15)?.ProductBarcode;
            if (code != null)
            {
                var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, code);
                Assert.False(res[code].Item1);
                Assert.True(res[code].Item2.Length > 0);
            }

        }

        [Fact]
        public void TestCheckBarcode_JoinOrder()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var code = inventoryContract.Inventorys.FirstOrDefault(i => i.Status == InventoryStatus.JoinOrder && i.StoreId == 15)?.ProductBarcode;
            if (code != null)
            {
                var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, code);
                Assert.False(res[code].Item1);
                Assert.True(res[code].Item2.Length > 0);
            }

        }



        [Fact]
        public void TestCheckBarcode_Purchased()
        {
            var inventoryContract = _fixture.Container.Resolve<IInventoryContract>();
            var code = inventoryContract.Inventorys.FirstOrDefault(i => i.Status >= InventoryStatus.PurchasStart && i.Status <= InventoryStatus.PurchasEnd && i.StoreId == 15)?.ProductBarcode;
            if (code != null)
            {
                var res = inventoryContract.CheckBarcodes(InventoryCheckContext.零售, 15, null, 9, code);
                Assert.False(res[code].Item1);
                Assert.True(res[code].Item2.Length > 0);
            }

        }




    }
}
