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
    public interface IAnnualLeaveContract : IDependency
    {
        #region IAnnualLeaveContract

        AnnualLeave View(int Id);

        AnnualLeaveDto Edit(int Id);

        IQueryable<AnnualLeave> AnnualLeaves { get; }

        OperationResult Insert(params AnnualLeaveDto[] dtos);

        OperationResult Update(params AnnualLeaveDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title);

        #endregion
    }
}
