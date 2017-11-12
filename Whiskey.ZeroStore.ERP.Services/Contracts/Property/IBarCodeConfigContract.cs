using System.Linq;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IBarCodeConfigContract : IDependency
    {
        BarCodeConfig View(int Id);

        BarCodeConfigDto Edit(int Id);

        IQueryable<BarCodeConfig> BarCodeConfigs { get; }

        OperationResult Insert(params BarCodeConfigDto[] dtos);

        OperationResult Update(params BarCodeConfigDto[] dtos);
    }
}
