using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppArticleContract : IDependency
    {
        #region IAppArticleContract

        AppArticle View(int Id);

        AppArticleDto Edit(int Id);

        IQueryable<AppArticle> AppArticles { get; }

        OperationResult Insert(params AppArticleDto[] dtos);

        OperationResult Update(params AppArticleDto[] dtos); 

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Recommend(params int[] Id);

        OperationResult CancleRecommend(params int[] Id);
        #endregion


        

        
    }
}
