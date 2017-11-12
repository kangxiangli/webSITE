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
    public partial interface IMemberActivityContract : IDependency
    {
        #region MemberRechargeActivity

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        MemberActivity View(int Id);
        /// <summary>
        /// 获取领域模型实体数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        MemberActivityDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<MemberActivity> MemberActivitys { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params MemberActivityDto[] dtos);

        OperationResult Update(params MemberActivityDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        bool CheckExists(Expression<Func<MemberActivity, bool>> predicate, int id = 0);

        /// <summary>
        /// 获取会员键值对列表
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        List<SelectListItem> SelectList(string title);

        #endregion
    }
}
