using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
   public interface IProductBuysaidAttributeContract:IDependency
    {
       IQueryable<BuysaidAttribute> BuysaidAttributes { get; }
       OperationResult Insert(params BuysaidAttribute[] buysaidAttributes);
    }
}
