using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppArticleItemContract : IDependency
    {
        #region IAppArticleItemContract

        AppArticleItem View(int Id);

        AppArticleItemDto Edit(int Id);

        IQueryable<AppArticleItem> AppArticleItems { get; }

        OperationResult Insert(params AppArticleItemDto[] dtos);

        OperationResult Update(params AppArticleItemDto[] dtos); 

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);        
         
        #endregion


        
    }
}
