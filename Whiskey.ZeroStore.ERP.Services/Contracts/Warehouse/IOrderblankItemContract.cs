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
    public interface IOrderblankItemContract : IDependency
    {
        IQueryable<OrderblankItem> OrderblankItems { get; }
        OperationResult Insert(OrderblankItem[] items);
        OperationResult Delete(int[] delids);

        OperationResult Update(OrderblankItem[] orderblankItem);
    }
}
