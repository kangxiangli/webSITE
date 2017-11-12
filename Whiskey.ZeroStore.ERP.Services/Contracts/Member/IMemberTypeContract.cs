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
    public interface IMemberTypeContract : IDependency
    {
        #region IMemberTypeContract

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        MemberType View(int Id);
        /// <summary>
        /// 获取领域模型实体数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        MemberTypeDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<MemberType> MemberTypes { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params MemberTypeDto[] dtos);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Update(params MemberTypeDto[] dtos);

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Recovery(params int[] ids);

        /// <summary>
        /// 删除数据（物理删除）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Enable(params int[] ids);

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Disable(params int[] ids);

        /// <summary>
        /// 校验数据
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool CheckExists(Expression<Func<MemberType, bool>> predicate, int id = 0);

        /// <summary>
        /// 获取会员键值对列表
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        List<SelectListItem> SelectList(string title);

        #endregion

    }
}
