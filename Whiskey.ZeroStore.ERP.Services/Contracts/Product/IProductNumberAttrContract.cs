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
    public interface IProductBigNumberAttrContract : IDependency
    {
       IQueryable<ProductBigNumberAttr> ProductNumberAttrs { get; }
       OperationResult Insert(params ProductBigNumberAttr[] prods);
       OperationResult Update(params ProductBigNumberAttr[] prods);
    }
}
