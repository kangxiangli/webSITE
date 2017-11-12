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
    public interface IAdministratorPermissionRelationContract : IDependency
    {
        AAdministratorPermissionRelation View(int Id);

        AAdministratorPermissionRelationDto Edit(int Id);

        IQueryable<AAdministratorPermissionRelation> AAdministratorPermissionRelations { get; }

        OperationResult Insert(params AAdministratorPermissionRelationDto[] dtos);
        OperationResult Insert(params AAdministratorPermissionRelation[] adminpers);

        OperationResult Update(params AAdministratorPermissionRelationDto[] dtos);
        OperationResult Update(params AAdministratorPermissionRelation[] adminpers);
        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
    }
}
