using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Services.Contracts;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{
   
    class ProductBuysaidAttributeService:ServiceBase, IProductBuysaidAttributeContract
    {
        protected readonly IRepository<BuysaidAttribute, int> _buysaidAttributeRepository;

        public ProductBuysaidAttributeService(IRepository<BuysaidAttribute, int> buysaidAttributeRepository):base(buysaidAttributeRepository.UnitOfWork)
        {
            _buysaidAttributeRepository = buysaidAttributeRepository;
        }
        public IQueryable<Models.Entities.Products.BuysaidAttribute> BuysaidAttributes
        {
            get { return _buysaidAttributeRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(params Models.Entities.Products.BuysaidAttribute[] buysaidAttributes)
        {
            return _buysaidAttributeRepository.Insert((IEnumerable < BuysaidAttribute > )buysaidAttributes) > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);
        }
    }
}
