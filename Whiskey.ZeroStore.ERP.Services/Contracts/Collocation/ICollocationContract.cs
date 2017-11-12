using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 定义业务层接口
    /// </summary>
    public partial interface ICollocationContract : IDependency
    {       
        /// <summary>
        /// 获取数据集合
        /// </summary>
        IQueryable<Collocation> Collocations {get; }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto">领域模型实体</param>
        /// <returns></returns>
        OperationResult Insert(params CollocationDto[] dtos);
        OperationResult Insert(params Collocation[] col);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="id">主键Id</param>
        /// <returns></returns>
        CollocationDto View(int id);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto">领域模型实体</param>
        /// <returns></returns>
        OperationResult Update(params CollocationDto[] dtos);
        OperationResult Update(params Collocation[] colls);
        /// <summary>
        /// 逻辑删除数据
        /// </summary>
        /// <param name="ids">主键id</param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Recovery(params int[] ids);
        /// <summary>
        /// 物理删除数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Enable(params int[] ids);
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">主键ID</param>
        /// <returns></returns>
        OperationResult Disable(params int[] ids);

        /// <summary>
        /// 获取键值对集合
        /// </summary>
        /// <param name="title">默认显示值</param>
        /// <returns></returns>
        List<SelectListItem> SelectList(string title);

        OperationResult GetFansList(int colloId, int PageIndex, int PageSize);

          
    }
}
