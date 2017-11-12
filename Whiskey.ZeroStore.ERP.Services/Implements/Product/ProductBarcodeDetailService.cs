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
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProductBarcodeDetailService : ServiceBase,IProductBarcodeDetailContract
    {
        protected readonly IRepository<ProductBarcodeDetail, int> _barcodeRepository;
        public ProductBarcodeDetailService(IRepository<ProductBarcodeDetail, int> barcodeRepository):base(barcodeRepository.UnitOfWork)
        {
            _barcodeRepository = barcodeRepository;
        }
        public IQueryable<ProductBarcodeDetail> productBarcodeDetails
        {
            get { return _barcodeRepository.Entities; }
        }

        public OperationResult BulkInsert(IEnumerable<ProductBarcodeDetail> details)
        {
            return _barcodeRepository.InsertBulk(details, a =>
           {
               a.OperatorId = AuthorityHelper.OperatorId;
               a.CreatedTime = DateTime.Now;
           });
        }

        public OperationResult BulkUpdate(IEnumerable<ProductBarcodeDetail> details)
        {
            return _barcodeRepository.UpdateBulk(details, a =>
            {
                a.OperatorId = AuthorityHelper.OperatorId;
                a.UpdatedTime = DateTime.Now;
            });
        }

        public OperationResult Insert(params ProductBarcodeDetail[] details)
        {
            details.Each(c=> { c.LogFlag = Guid.NewGuid().ToString().Replace("-","");c.ProductOperationLogs=new List<ProductOperationLog>()
            {
                new ProductOperationLog()
                {
                    ProductNumber = c.ProductNumber,
                    OnlyFlag=c.OnlyFlag,
                    ProductBarcode = c.ProductNumber+c.OnlyFlag,
                    LogFlag = c.LogFlag,
                    Description = "打印条码",
                    OperatorId=AuthorityHelper.OperatorId,
                    CreatedTime = c.CreatedTime
                }
            }; });
           return _barcodeRepository.Insert((IEnumerable<ProductBarcodeDetail>) details)>0?new OperationResult(OperationResultType.Success) :new OperationResult(OperationResultType.Error) ;
        }

        public OperationResult Update(params ProductBarcodeDetail[] details)
        {
           return _barcodeRepository.Update(details);
        }
    }
}
