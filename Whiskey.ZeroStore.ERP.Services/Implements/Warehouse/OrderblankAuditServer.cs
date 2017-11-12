using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Warehouse
{
    public class OrderblankAuditServer : ServiceBase, IOrderblankAuditContract
    {
        protected IRepository<OrderblankAudit, int> _orderblankAuditRepository;
        public OrderblankAuditServer(IRepository<OrderblankAudit, int> orderblankAuditRepository)
            : base(orderblankAuditRepository.UnitOfWork)
        {
            _orderblankAuditRepository = orderblankAuditRepository;
        }
       

        public Utility.Data.OperationResult Insert(Models.OrderblankAudit[] ordeblankArr)
        {
            return _orderblankAuditRepository.Insert((IEnumerable<OrderblankAudit>)ordeblankArr) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public Utility.Data.OperationResult Delete(params Models.OrderblankAudit[] orderblankArr)
        {
            throw new NotImplementedException();
        }

        public Utility.Data.OperationResult Update(params Models.OrderblankAudit[] orderblankArr)
        {
           return _orderblankAuditRepository.Update((ICollection<OrderblankAudit>)orderblankArr);
        }

        public IQueryable<OrderblankAudit> OrderblankAudits
        { 
            get { return _orderblankAuditRepository.Entities; }
        }
    }
}
