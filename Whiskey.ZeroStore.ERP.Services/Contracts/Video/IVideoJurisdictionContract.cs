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
   public interface IVideoJurisdictionContract: IDependency
    {
        IQueryable<VideoJurisdiction> VideoJurisdictions { get; }
        OperationResult Insert(params VideoJurisdiction[] jurisdictions);
        OperationResult Update(params VideoJurisdictionDto[] jurisdictions);
        OperationResult Remove(params int[] ids);
    }
}
