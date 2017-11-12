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
    public interface ILeaveInfoContract : IDependency
    {

        #region ILeaveInfoContract

        LeaveInfo View(int Id);

        LeaveInfoDto Edit(int Id);

        IQueryable<LeaveInfo> LeaveInfos { get; }

        OperationResult Insert(params LeaveInfoDto[] dtos);

        OperationResult Insert_API(params LeaveInfoDto[] dtos);

        OperationResult Update(params LeaveInfoDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Verify(LeaveInfoDto dto);        

        #endregion


    }
}
