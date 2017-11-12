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
    public interface IWorkLogAttributeContract : IDependency
    {
        #region IWorkLogAttributeContract

        WorkLogAttribute View(int Id);

        WorkLogAttributeDto Edit(int Id);

        IQueryable<WorkLogAttribute> WorkLogAttributes { get; }

        OperationResult Insert(params WorkLogAttributeDto[] dtos);

        OperationResult Update(params WorkLogAttributeDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title);

        List<SelectListItem> SelectGroupList(string title);
        
        #endregion

    }
}
