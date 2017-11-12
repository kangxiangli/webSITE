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
    public interface IPartnerContract : IDependency
    {
        #region IPartnerContract

        Partner View(int Id);

        PartnerDto Edit(int Id);

        IQueryable<Partner> Partners { get; }

        OperationResult Insert(params PartnerDto[] dtos);

        OperationResult Update(params PartnerDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title);

        #endregion
    }
}
