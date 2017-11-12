
using System.Collections.Generic;
using System.Linq;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ICollocationTemplateContract : IDependency
    {
        IQueryable<CollocationTemplate> Entities { get; }

        CollocationTemplate View(int Id);

        OperationResult Insert(params CollocationTemplate[] entities);

        OperationResult Update(params CollocationTemplate[] entities);
        /// <summary>
        /// 启用或禁用数据
        /// </summary>
        /// <param name="enable">true启动,false禁用</param>
        /// <param name="ids">数据Ids</param>
        /// <returns></returns>
        OperationResult EnableOrDisable(bool enable, params int[] ids);
        /// <summary>
        /// 删除或还原数据
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult DeleteOrRecovery(bool delete, params int[] ids);


        OperationResult Delete(params CollocationTemplate[] entities);

        OperationResult Update(ICollection<CollocationTemplate> entities);

        CollocationTemplate Edit(int id);

    }
}




