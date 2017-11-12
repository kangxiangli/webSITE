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
   public interface IResignationToExamineContract : IDependency
    {
        IQueryable<ResignationToExamine> EntryToExamines { get; }

        OperationResult Insert(params ResignationToExamine[] dtos);
    }
}
