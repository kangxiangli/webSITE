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
    public interface IWorkTimeDetaileContract : IDependency
    {
        IQueryable<WorkTimeDetaile> WorkTimeDetailes { get; }
        OperationResult Insert(params WorkTimeDetaile[] rules);
        OperationResult TrueRemove(int year, int month, params int[] ids);
        OperationResult Update(params WorkTimeDetaileDto[] rules);
        OperationResult UpdateWorkType(int ToExamineStatues, int day, params int[] ids);
        OperationResult TrueRemoveByWorkTimeId(params int[] ids);
        WorkTimeDetaileDto Edit(int Id);
    }
}
