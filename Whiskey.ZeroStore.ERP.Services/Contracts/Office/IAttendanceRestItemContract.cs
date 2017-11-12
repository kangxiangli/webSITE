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
    public interface IAttendanceRestItemContract : IDependency
    {
        #region IAttendanceRestItemContract

        AttendanceRestItem View(int Id);

        AttendanceRestItemDto Edit(int Id);

        IQueryable<AttendanceRestItem> AttendanceRestItems { get; }

        OperationResult Insert(params AttendanceRestItemDto[] dtos);

        OperationResult Update(params AttendanceRestItemDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);         

        #endregion

    }
}
