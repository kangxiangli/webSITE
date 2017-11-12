using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
 
namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 业务操作接口
    /// </summary>
    public interface IHtmlPartContract : IDependency
    {
        #region IHtmlPartContract

        /// <summary>
        /// 获取编辑数据对象
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        HtmlPartDto Edit(int Id);
        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<HtmlPart> HtmlParts { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        OperationResult Insert(params HtmlPartDto[] JSDtos);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="templateJS"></param>
        /// <returns></returns>
        OperationResult Update(params HtmlPartDto[] JSDtos);        

        /// <summary>
        /// 删除数据（物理）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        /// <summary>
        /// 获取JS集合
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        List<SelectListItem> SelectList(string title);
              
        HtmlPart View(int id);

        #endregion
    }
}
