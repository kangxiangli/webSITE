using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;


namespace Whiskey.ZeroStore.ERP.Services.Contracts.Logs
{
    public interface IProductOperationLogContract : IDependency
    {
        IQueryable<ProductOperationLog> ProductLogs { get; }
        OperationResult Insert(params ProductOperationLog[] lgos);


    }
}
