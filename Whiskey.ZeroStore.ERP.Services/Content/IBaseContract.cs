using System.Linq;
using Whiskey.Core;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Services
{
    public interface IBaseContract<T> : IDependency
    {
        IQueryable<T> Entities { get; }

        T View(int Id);

        OperationResult Insert(params T[] entities);

        OperationResult Update(params T[] entities);
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

    }

    public interface IBaseContract<T, TDto> : IBaseContract<T>
    {
        OperationResult Insert(params TDto[] dtos);

        OperationResult Update(params TDto[] dtos);

        TDto Edit(int Id);
    }
}
