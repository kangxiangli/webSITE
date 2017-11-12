using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Services.Content;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace UnitTest
{
    public class CacheAccessUnitTest : IClassFixture<AutofacFixture>
    {
        AutofacFixture _fixture;
        public CacheAccessUnitTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void Test_GetEnabledStores()
        {
            var storageContract = _fixture.Container.Resolve<IStorageContract>();
            var adminContract = _fixture.Container.Resolve<IAdministratorContract>();
            var storeContract = _fixture.Container.Resolve<IStoreContract>();
            //CacheAccess.GetAllStoreListItem(storeContract, true);
            //CacheAccess.GetAllStorageListItem(storageContract, true);

            //CacheAccess.GetManagedStoreListItem(adminContract,true);
            //CacheAccess.GetManagedStorageListItem(storageContract, adminContract);

            //CacheAccess.GetManagedStorageByStoreId(storageContract, adminContract, 15, true);
            //CacheAccess.GetManagedStorage(storageContract, adminContract);
           
          
        }
    }
}
