
using System.Linq;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IWorkOrderCategoryContract : IBaseContract<WorkOrderCategory, WorkOrderCategoryDto>
    {

        #region ILeaveInfoContract

        IQueryable<WorkOrderCategory> WorkOrderCategorys { get; }

        #endregion
    }
}

