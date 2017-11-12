using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse
{
    public interface IProductBarcodePrintInfoContract : IDependency
    {
        IQueryable<ProductBarcodePrintInfo> ProductBarcodePrintInfos { get; }


        OperationResult Insert(bool trans, params ProductBarcodePrintInfo[] ProductBarcodePrintInfos);


        OperationResult Update(ProductBarcodePrintInfo[] dtos, bool isTrans = false);

        OperationResult Remove(params int[] ids);
    }
}
