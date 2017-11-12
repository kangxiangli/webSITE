using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProductBarcodePrintInfoService : ServiceBase, IProductBarcodePrintInfoContract
   {
        private IRepository<ProductBarcodePrintInfo, int> _productBarcodePrintInfoRepository;

        public ProductBarcodePrintInfoService(IRepository<ProductBarcodePrintInfo, int> productBarcodePrintInfoRepository)
            : base(productBarcodePrintInfoRepository.UnitOfWork)
       {
           _productBarcodePrintInfoRepository = productBarcodePrintInfoRepository;
       }


        public IQueryable<ProductBarcodePrintInfo> ProductBarcodePrintInfos
        {
            get { return _productBarcodePrintInfoRepository.Entities; }
        }

        public OperationResult Insert(bool trans, params ProductBarcodePrintInfo[] ProductBarcodePrintInfos)
        {
            ProductBarcodePrintInfos.Each(c=>c.LastUpdateTime=DateTime.Now);
            if (trans)
                _productBarcodePrintInfoRepository.UnitOfWork.TransactionEnabled = true;
            return _productBarcodePrintInfoRepository.Insert((IEnumerable<ProductBarcodePrintInfo>)ProductBarcodePrintInfos) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(ProductBarcodePrintInfo[] dtos, bool isTrans = false)
        {
            dtos.Each(c=>c.LastUpdateTime=DateTime.Now);
            if (isTrans)
                _productBarcodePrintInfoRepository.UnitOfWork.TransactionEnabled = true;
            return _productBarcodePrintInfoRepository.Update(dtos);
        }

        public OperationResult Remove(params int[] ids)
        {
            throw new NotImplementedException();
        }

       
   }
}
