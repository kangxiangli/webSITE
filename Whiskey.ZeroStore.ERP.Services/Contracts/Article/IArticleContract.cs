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
    public interface IArticleContract : IDependency
    {
        #region Article
        
        Article View(int Id);

        ArticleDto Edit(int Id);

        IQueryable<Article> Articles { get; }        

        OperationResult Insert(params ArticleDto[] dto);

        OperationResult Update(params ArticleDto[] dto);

         
        /// <summary>
        /// 批量设置属性
        /// </summary>
        /// <param name="strIds">主键ID</param>
        /// <param name="article">属性的载体</param>
        /// <returns></returns>
        OperationResult SetAttrubute(string strIds, Article article);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);         

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult Verify(int approvalStatus,params int[] ids);

        OperationResult Product(int[] Id);

        #endregion







    }
}
