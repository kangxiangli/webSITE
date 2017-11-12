using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services
{
   //yxk 2015-11
   public interface IOrderblankAuditContract:IDependency
    {
        IQueryable<OrderblankAudit> OrderblankAudits { get; }
        OperationResult Insert(OrderblankAudit[] ordeblankArr);
        OperationResult Delete(params OrderblankAudit[] orderblankArr);
        OperationResult Update(params OrderblankAudit[] orderblankArr);

        
    }
}
