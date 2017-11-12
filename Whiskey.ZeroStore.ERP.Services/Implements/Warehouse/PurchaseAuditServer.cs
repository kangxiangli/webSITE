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

namespace Whiskey.ZeroStore.ERP.Services.Implements.Warehouse
{
    //yxk 2015-11
   public class PurchaseAuditServer:ServiceBase,IPurchaseAuditContract
    {
       private readonly IRepository<PurchaseAudit, int> _purchaseAuditRepository;
       public PurchaseAuditServer(IRepository<PurchaseAudit,int> purchaseAuditRepository) :base
           (purchaseAuditRepository.UnitOfWork)
       {
           _purchaseAuditRepository = purchaseAuditRepository;
       }
        public IQueryable<Models.Entities.PurchaseAudit> PurchaseAudits
        {
            get { return _purchaseAuditRepository.Entities; }
        }

        public OperationResult Insert(params Models.Entities.PurchaseAudit[] puraudits)
        {
           return _purchaseAuditRepository.Insert((IEnumerable<PurchaseAudit>)puraudits)>0?new OperationResult(OperationResultType.Success):new OperationResult(OperationResultType.Error);
        }
    }
}
