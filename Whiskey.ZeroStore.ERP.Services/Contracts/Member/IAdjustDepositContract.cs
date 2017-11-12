using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAdjustDepositContract : IDependency
    {
        #region AdjustDeposit

        /// <summary>
        /// 获取实体数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        AdjustDeposit View(int Id);
        /// <summary>
        /// 获取领域模型实体数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        AdjustDepositDto Edit(int Id);

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<AdjustDeposit> AdjustDeposits { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params AdjustDepositDto[] dtos);

        OperationResult Update(params AdjustDepositDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        bool CheckExists(Expression<Func<AdjustDeposit, bool>> predicate, int id = 0);

        OperationResult Verify( bool contentToTitle, Action<List<int>> sendNotificationAction, params int[] Id);

        #endregion


        
    }
}
