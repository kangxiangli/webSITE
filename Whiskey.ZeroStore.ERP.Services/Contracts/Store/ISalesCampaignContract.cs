using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ISalesCampaignContract : IDependency
    {
        IQueryable<SalesCampaign> SalesCampaigns { get;  }
        OperationResult Insert(bool trans,params SalesCampaign[] sales);
      
        OperationResult Update(params SalesCampaign[] sales);

        OperationResult Disable(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        List<SalesCampaign> GetAvailableSalesCampaignsByStore(int storeId);
    }
}
