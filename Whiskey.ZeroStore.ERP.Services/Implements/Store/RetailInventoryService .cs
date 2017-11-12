using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Implements;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    //public class RetailInventoryService : DefaultService<RetailInventory>, IRetailInventoryContract
    public class RetailInventoryService : ServiceBase, IRetailInventoryContract
    {
        //private IRepository<RetailInventory, int> _repo;
        //public RetailInventoryService(IRepository<RetailInventory,int> repo):base(repo)
        //{

        //}
        protected IRepository<RetailInventory, int> _retailInventoryRepository;

        public RetailInventoryService(IRepository<RetailInventory, int> retailInventoryRepository) :base(retailInventoryRepository.UnitOfWork)
       {
            _retailInventoryRepository = retailInventoryRepository;
        }
        public IQueryable<RetailInventory> RetailInventorys
        {
            get { return _retailInventoryRepository.Entities; }
        }
    }
}
