using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation;
using Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation;

namespace Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation
{
   public interface IStoreCollocationInfoContract : IDependency
    {
        IQueryable<StoreCollocationInfo> StoreCollocationInfos { get; }
        OperationResult Insert(params StoreCollocationInfo[] rules);
        OperationResult Update(params StoreCollocationInfoDto[] rules);
        OperationResult Remove(bool statues, params int[] ids);
        OperationResult DeleteByCollocationId(params int[] ids);
        OperationResult Disable(bool statues, params int[] ids);
        OperationResult TrueRemove(params int[] ids);
    }
}
