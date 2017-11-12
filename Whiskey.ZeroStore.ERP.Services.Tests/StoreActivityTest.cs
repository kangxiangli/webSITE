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

namespace UnitTest
{
    public class StoreActivityTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public StoreActivityTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        public void TestStoreRecommends()
        {
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var query = contract.StoreActivities;
            var count = query.Count();
            Assert.NotNull(contract);
            Assert.NotNull(query);
         
        }

        [Fact]
        public void TestInsert()
        {
           
            var storeEntity = _fixture.Container.Resolve<IStoreContract>().View(14);
            var memberTypes = _fixture.Container.Resolve<IMemberTypeContract>().MemberTypes.ToList();
            var entity = new StoreActivity()
            {
                ActivityName = "测试",
                StartDate = DateTime.Now.AddDays(-1),
                EndDate = DateTime.Now.AddYears(1),
                MinConsume = 5000M,
                Notes = "mark",
                OperatorId = 9,
            };
            entity.MemberTypes = "1";
            entity.StoreIds = "14";

            
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();

            var res = contract.Insert(entity);
            Assert.True(res.ResultType == OperationResultType.Success);

        }

        [Fact]
        public void TestDisable()
        {
            var id = new int[] { 1};
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var res = contract.Disable(id);
            Assert.True(res.ResultType == OperationResultType.Success);
        }

        [Fact]
        public void TestEnable()
        {
            var id = new int[] { 1 };
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var res = contract.Enable(id);
            Assert.True(res.ResultType == OperationResultType.Success);
        }

        [Fact]
        public void TestDelete()
        {
            var id = new int[] { 1 };
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var res = contract.Delete(id);
            var entities = contract.StoreActivities.Where(s => id.Contains(s.Id)).ToList();
            Assert.True(res.ResultType == OperationResultType.Success);
            foreach (var item in entities)
            {
                Assert.True(item.IsDeleted);
            }
            
        }

        [Fact]
        public void TestRecovery()
        {
            var id = new int[] { 1 };
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var res = contract.Recovery(id);
            var entities = contract.StoreActivities.Where(s => id.Contains(s.Id)).ToList();
            Assert.True(res.ResultType == OperationResultType.Success);
            foreach (var item in entities)
            {
                Assert.True(item.IsDeleted==false);
            }

        }

        [Fact]
        public void TestUpdate()
        {
            var ids = new int[] { 1  };
            var contract = _fixture.Container.Resolve<IStoreActivityContract>();
            var list = contract.StoreActivities.Where(i => ids.Contains(i.Id)).ToList();
            foreach (var item in list)
            {
                item.IsDeleted = false;
                item.IsEnabled = true;
            }

            var res = contract.Update(list.ToArray());
            Assert.True(res.ResultType == OperationResultType.Success);
            var list2 = contract.StoreActivities.Where(i => ids.Contains(i.Id)).ToList();
            foreach (var item in list2)
            {
                Assert.True(item.IsDeleted == false);
                Assert.True(item.IsEnabled == true);
            }

        }
    }
}
