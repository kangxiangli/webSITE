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
    public interface IFieldContract : IDependency
    {
        #region IFieldContract

        Field View(int Id);

        FieldDto Edit(int Id);

        IQueryable<Field> Fields { get; }

        OperationResult Insert(params FieldDto[] dtos);

        OperationResult Update(params FieldDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Verify(FieldDto dto);

        #endregion

    }
}
