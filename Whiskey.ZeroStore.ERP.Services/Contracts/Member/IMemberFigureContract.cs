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
    public interface IMemberFigureContract : IDependency
    {
        #region IMemberFigureContract

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        MemberFigure View(int Id);
        /// <summary>
        /// 获取领域模型实体数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        MemberFigureDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<MemberFigure> MemberFigures { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params MemberFigureDto[] dtos);

        OperationResult Update(params MemberFigureDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);


        #endregion

    }
}
