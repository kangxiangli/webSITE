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
    public interface IRestContract : IDependency
    {

        #region IRestContract

        Rest View(int Id);

        RestDto Edit(int Id);

        IQueryable<Rest> Rests { get; }

        OperationResult Insert(params RestDto[] dtos);

        OperationResult Update(params RestDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Update(Rest rest);

        OperationResult Reset(int? departmentId = 0);

        #endregion

    }
}
