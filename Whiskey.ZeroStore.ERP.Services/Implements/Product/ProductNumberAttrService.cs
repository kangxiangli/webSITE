using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProductNumberAttrService : IProductBigNumberAttrContract
    {
        protected readonly IRepository<ProductBigNumberAttr, int> _productNumbAttrRepository;
        public ProductNumberAttrService(IRepository<ProductBigNumberAttr, int> productNumbAttrRepository)
       {
           _productNumbAttrRepository = productNumbAttrRepository;
       }
        public IQueryable<Models.Entities.ProductBigNumberAttr> ProductNumberAttrs
        {
            get {
                return _productNumbAttrRepository.Entities;
            }
        }

        public Utility.Data.OperationResult Insert(params Models.Entities.ProductBigNumberAttr[] prods)
        {
            return _productNumbAttrRepository.Insert((IEnumerable<ProductBigNumberAttr>)prods) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public Utility.Data.OperationResult Update(params Models.Entities.ProductBigNumberAttr[] prods)
        {
           return _productNumbAttrRepository.Update(prods);
        }
    }
}
