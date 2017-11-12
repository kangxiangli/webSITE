using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
   public interface IEntryContract : IDependency
    {
        IQueryable<Entry> Entrys { get; }

        OperationResult Insert(params Entry[] dtos);

        OperationResult Update(params EntryDto[] dtos);

        OperationResult ToExamine(int ToExamineStatues, params int[] ids);

        OperationResult RoleJurisdiction(string JurisdictionStr, string InterviewEvaluation,params int[] ids);
        OperationResult Update(params Entry[] entities);
    }
}
