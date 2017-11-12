using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Data;
using System.Linq.Expressions;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ITimeoutConfigContract : IDependency
    {
        TimeoutConfig View(int Id);

        TimeoutConfigDto Edit(int Id);

        IQueryable<TimeoutConfig> TimeoutConfigs { get; }

        OperationResult Insert(params TimeoutConfigDto[] dtos);
        OperationResult Insert(params TimeoutConfig[] timeouts);

        OperationResult Update(params TimeoutConfigDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        bool CheckExists(Expression<Func<TimeoutConfig, bool>> predicate, int id = 0);

        OperationResult Update(params TimeoutConfig[] roles);
    }
}
