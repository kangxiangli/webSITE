using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;


namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberSignContract : IDependency
    {
        #region MemberSign

        MemberSign View(int Id);

        MemberSignDto Edit(int Id);

        IQueryable<MemberSign> MemberSigns { get; }

        OperationResult Insert(params MemberSignDto[] dtos);

        OperationResult Update(params MemberSignDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

      
        #endregion

    }
}
