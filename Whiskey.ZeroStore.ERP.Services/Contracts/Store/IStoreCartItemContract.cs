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
    
    public interface IStoreCartItemContract: IDependency
    {
        #region StoreCartItem

        OperationResult Remove(params int[] ids);

        OperationResult Update(params StoreCartItem[] StoreCartItems);
        
        #endregion

    }
}
