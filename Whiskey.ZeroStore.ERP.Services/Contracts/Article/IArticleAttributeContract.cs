using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IArticleAttributeContract : IDependency
    {
        #region ArticleAttribute

        ArticleAttribute View(int Id);

        ArticleAttributeDto Edit(int Id);

        IQueryable<ArticleAttribute> ArticleAttributes { get; }        

        OperationResult Insert(params ArticleAttributeDto[] dtos);

        OperationResult Update(params ArticleAttributeDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title);
        #endregion


        
    }
}
