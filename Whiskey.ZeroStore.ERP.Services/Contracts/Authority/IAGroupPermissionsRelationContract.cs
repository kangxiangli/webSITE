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
    public interface IAGroupPermissionsRelationContract : IDependency
    {
        AGroupPermissionRelation View(int Id);

        AGroupPermissionsRelationDto Edit(int Id);

        IQueryable<AGroupPermissionRelation> AGroupPermissionsRelations { get; }

        OperationResult Insert(params AGroupPermissionsRelationDto[] dtos);
        OperationResult Insert(params AGroupPermissionRelation[] adminpers);

        OperationResult Update(params AGroupPermissionsRelationDto[] dtos);
        OperationResult Update(params AGroupPermissionRelation[] adminpers);
        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
    }
}
