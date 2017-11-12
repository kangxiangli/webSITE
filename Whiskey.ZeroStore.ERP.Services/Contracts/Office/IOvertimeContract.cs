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
    public interface IOvertimeContract : IDependency
    {
        #region IOvertimeContract

        Overtime View(int Id);

        OvertimeDto Edit(int Id);

        IQueryable<Overtime> Overtimes { get; }

        OperationResult Insert(params OvertimeDto[] dtos);

        OperationResult Update(params OvertimeDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Verify(OvertimeDto dto);

        OperationResult ExchangeOvertime(int Id);

        IQueryable<OvertimeRestItem> OvertimeRestItems { get; }

        #endregion


    }
}
