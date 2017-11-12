using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IArticleItemContract : IDependency
    {
        #region ArticleItem

        ArticleItem View(int Id);

        ArticleItemDto Edit(int Id);

        IQueryable<ArticleItem> ArticleItems { get; }
        
        OperationResult Insert(params ArticleItemDto[] dtos);

        OperationResult Update(params ArticleItemDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult SetShow(int Id);

        List<SelectListItem> SelectList(string p);

        List<SelectListItem> SelectList(string title, int parentId);

        List<SelectListItem> SelectAllList(string title);
        #endregion



        
    }
}
