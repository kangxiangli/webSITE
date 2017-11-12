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
    public interface IPartnerExperienceContract : IDependency
    {
        #region IPartnerExperienceContract

        PartnerExperience View(int Id);

        PartnerExperienceDto Edit(int Id);

        IQueryable<PartnerExperience> PartnerExperiences { get; }

        OperationResult Insert(params PartnerExperienceDto[] dtos);

        OperationResult Update(params PartnerExperienceDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);        

        #endregion
    }
}
