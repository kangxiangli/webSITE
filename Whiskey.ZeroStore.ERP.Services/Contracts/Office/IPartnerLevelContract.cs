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
    public interface IPartnerLevelContract : IDependency
    {
        #region IPartnerLevelContract

        PartnerLevel View(int Id);

        PartnerLevelDto Edit(int Id);

        IQueryable<PartnerLevel> PartnerLevels { get; }

        OperationResult Insert(params PartnerLevelDto[] dtos);

        OperationResult Update(params PartnerLevelDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult OrderLevel(int Id, int PartnerId);
        #endregion

        
    }
}
