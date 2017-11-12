using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Threading;

namespace UnitTest
{
    public class InventoryLockTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public InventoryLockTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestCanLockInventory()
        {
            var dtos = new List<LockInventoryDto>()
            {
                new LockInventoryDto()
                {
                    OperatorId  = 9 ,
                    ProductBarcode = "AAABBB0001",
                    
                },
                new LockInventoryDto()
                {
                    OperatorId  = 9 ,
                    ProductBarcode = "AAABBB0002",
                },
                new LockInventoryDto()
                {
                    OperatorId  = 9 ,
                    ProductBarcode = "AAABBB0003",
                }
            };
            var contract = _fixture.Container.Resolve<IInventoryContract>();
            var lockPeriod = TimeSpan.FromSeconds(10);
            contract.SetInventoryLocked(lockPeriod, dtos.ToArray());

            //对其他人锁定
            foreach (var item in dtos)
            {
                var hasLock = contract.IsInventoryDisable(item.ProductBarcode,8);
                Assert.True(hasLock);
            }

            //对操作人不锁定
            foreach (var item in dtos)
            {
                var hasLock = contract.IsInventoryDisable(item.ProductBarcode, 9);
                Assert.False(hasLock);
            }

            //过期后,对所有人开放
            Thread.Sleep(lockPeriod);

            foreach (var item in dtos)
            {
                Assert.False(contract.IsInventoryDisable(item.ProductBarcode,9));
            }
            foreach (var item in dtos)
            {
                Assert.False(contract.IsInventoryDisable(item.ProductBarcode, 8));
            }
        }
    }
}
