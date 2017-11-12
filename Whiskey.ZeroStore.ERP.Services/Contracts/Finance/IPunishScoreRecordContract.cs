using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IPunishScoreRecordContract : IDependency
    {
        IQueryable<PunishScoreRecord> Entities { get; }

     
        OperationResult Insert(params PunishScoreRecord[] entity);


    }
}
