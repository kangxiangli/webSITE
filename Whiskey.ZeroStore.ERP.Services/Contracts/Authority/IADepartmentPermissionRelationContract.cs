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
    public interface IADepartmentPermissionRelationContract : IDependency
    {
        ADepartmentPermissionRelation View(int Id);

        ADepartmentPermissionRelationDto Edit(int Id);

        IQueryable<ADepartmentPermissionRelation> ADepartmentPermissionRelations { get; }

        OperationResult Insert(params ADepartmentPermissionRelationDto[] dtos);
        OperationResult Insert(params ADepartmentPermissionRelation[] adminpers);

        OperationResult Update(params ADepartmentPermissionRelationDto[] dtos);
        OperationResult Update(params ADepartmentPermissionRelation[] adminpers);
        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
    }
}
