using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts.Logs;

namespace Whiskey.ZeroStore.ERP.Services.Logs
{
    public class ProductOperationLogServer : ServiceBase, IProductOperationLogContract
    {

        protected readonly IRepository<ProductOperationLog, int> _productLogRepository;

        public ProductOperationLogServer(IRepository<ProductOperationLog, int> productLogRepository)
            : base(productLogRepository.UnitOfWork)
        {
            _productLogRepository = productLogRepository;
        }
        public IQueryable<ProductOperationLog> ProductLogs
        {
            get { return _productLogRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(params ProductOperationLog[] logs)
        {


            logs.Each(c =>
            {
                c.LogFlag = Guid.NewGuid().ToString().Replace("-","");
                c.CreatedTime = DateTime.Now;
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.IsDeleted = false;
                c.IsEnabled = true;
            });
            return _productLogRepository.Insert((IEnumerable<ProductOperationLog>)logs) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);

        }
    }
}
