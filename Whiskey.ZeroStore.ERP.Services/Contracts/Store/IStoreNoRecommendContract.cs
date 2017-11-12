using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IStoreNoRecommendContract : IDependency
    {
        IQueryable<StoreNoRecommend> StoreNoRecommends { get; }
        OperationResult Insert(params StoreNoRecommend[] entities);
        OperationResult Disable(int[] ids);
        OperationResult Enable(int[] ids);
        OperationResult Delete(int[] ids);
        OperationResult Recovery(int[] ids);
        OperationResult Update(params StoreNoRecommend[] entities);

    }
}
