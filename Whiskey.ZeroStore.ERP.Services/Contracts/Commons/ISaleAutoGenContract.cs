
using System;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ISaleAutoGenContract : IBaseContract<SaleAutoGen, SaleAutoGenDto>
    {
        OperationResult Insert(Action<bool, string, int[]> process, params SaleAutoGenDto[] dtos);
    }
}

