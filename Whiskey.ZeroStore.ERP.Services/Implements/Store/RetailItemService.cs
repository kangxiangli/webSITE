using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implement
{
   public class RetailItemService : ServiceBase, IRetailItemContract
   {

       protected IRepository<RetailItem, int> _retailItemRepository;

       public RetailItemService(IRepository<RetailItem, int> retailItemRepository):base(retailItemRepository.UnitOfWork)
       {
           _retailItemRepository = retailItemRepository;
       }
       public IQueryable<RetailItem> RetailItems
        {
            get { return _retailItemRepository.Entities; }
        }

        public OperationResult Insert(List<RetailItem> retails)
        {
           int res= _retailItemRepository.Insert((IEnumerable<RetailItem>) retails);
            return res > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);

        }

   }
}
