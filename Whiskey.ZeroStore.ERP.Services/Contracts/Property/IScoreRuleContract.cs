using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Transfers.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IScoreRuleContract : IDependency
    {
        IQueryable<ScoreRule> ScoreRules { get;}
        OperationResult Insert(params ScoreRule[] rules);
        OperationResult Update(params ScoreRuleDto[] rules);
        OperationResult Enable(int id);

        OperationResult Disable(params int[] ids);
        OperationResult Remove(params int[] ids);
        OperationResult Recovery(params int[] ids);

        /// <summary>
        /// 当前使用的积分规则
        /// </summary>
        ScoreRule Current { get; }
    }
}
