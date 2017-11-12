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
    public interface IWorkTimeContract : IDependency
    {
        #region IWorkTimeContract

        WorkTime View(int Id);

        WorkTimeDto Edit(int Id);

        IQueryable<WorkTime> WorkTimes { get; }

        OperationResult Insert(params WorkTimeDto[] dtos);

        OperationResult Update(params WorkTimeDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title);

        #endregion
    }
}
