using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IGroupContract : IDependency
    {
        IQueryable<Group> Groups { get; }
        OperationResult Insert(params Group[] groups);

        OperationResult Update(params Group[] groups);

        OperationResult Remove(params int[] ids);

    }
}
