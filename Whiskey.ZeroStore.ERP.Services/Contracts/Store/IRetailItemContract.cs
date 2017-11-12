using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IRetailItemContract : IDependency
    {
        IQueryable<RetailItem> RetailItems { get; }
        OperationResult Insert(List<RetailItem> retails);
    }
}
