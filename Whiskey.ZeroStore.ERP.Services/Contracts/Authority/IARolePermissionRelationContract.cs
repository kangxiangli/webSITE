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
    public interface IARolePermissionRelationContract : IDependency
    {
        ARolePermissionRelation View(int Id);

        ARolePermissionRelationDto Edit(int Id);

        IQueryable<ARolePermissionRelation> ARolePermissionRelations { get; }

        OperationResult Insert(params ARolePermissionRelationDto[] dtos);
        OperationResult Insert(params ARolePermissionRelation[] adminpers);

        OperationResult Update(params ARolePermissionRelationDto[] dtos);
        OperationResult Update(params ARolePermissionRelation[] adminpers);
        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
    }
}
