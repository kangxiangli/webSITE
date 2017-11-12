using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
   public interface  IStoreValueRuleContract : IDependency
    {
        IQueryable<StoreValueRule> StoreValueRules { get; }
        OperationResult Insert(int ruleType, params StoreValueRuleDto[] rules);
        OperationResult Update(params StoreValueRuleDto[] rules);
        OperationResult Update(params StoreValueRule[] rules);
        OperationResult Enable(int id,int ruleType);

        OperationResult Disable(params int[] ids);
        OperationResult Remove(params int[] ids);
        OperationResult Recovery(params int[] ids);
        StoreValueRule View(int id);
        StoreValueRuleDto Edit(int Id);
    }
}
