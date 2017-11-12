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
   public  interface IProductTrackContract : IDependency
    {
        IQueryable<ProductTrack> Tracks { get; }
        OperationResult Insert(params ProductTrackDto[] rules);

        OperationResult BulkInsert(params ProductTrack[] entities);

        OperationResult BulkInsert(IEnumerable<ProductTrack> entities);
    }
}
