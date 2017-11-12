using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IStoreSpendStatisticsContract : IDependency
    {
        IQueryable<StoreSpendStatistics> StoreStatistics { get; }
        OperationResult Insert(params StoreSpendStatistics[] ents);
    }
}
