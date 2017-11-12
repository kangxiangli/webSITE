using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 业务操作接口
    /// </summary>
    public interface IHtmlItemContract : IDependency
    {
        #region IHtmlItemContract

        /// <summary>
        /// 获取编辑数据对象
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        HtmlItemDto Edit(int Id);
        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<HtmlItem> HtmlItems { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        OperationResult Insert(params HtmlItemDto[] JSDtos);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="templateJS"></param>
        /// <returns></returns>
        OperationResult Update(params HtmlItemDto[] JSDtos);
        

        /// <summary>
        /// 删除数据（物理）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        List<OperationResult> Insert(HttpFileCollectionBase listFile, HtmlItemDto dto, HtmlItemFlag flag, string savePath);

        /// <summary>
        /// 获取JS集合
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        List<Values<string, string>> SelectList(string title);
              

        List<OperationResult> Update(HttpFileCollectionBase listFile, HtmlItemDto JSDto, HtmlItemFlag htmlItemFlag, string jsPath);

        HtmlItem View(int id);

        #endregion
    }
}
