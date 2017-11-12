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
    public interface IWorkLogContract: IDependency
    {
        #region IWorkLogContract

        WorkLog View(int Id);

        WorkLogDto Edit(int Id);

        IQueryable<WorkLog> WorkLogs { get; }

        OperationResult Insert(params WorkLogDto[] dtos);

        OperationResult Update(params WorkLogDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);        

        #endregion

    }
}
