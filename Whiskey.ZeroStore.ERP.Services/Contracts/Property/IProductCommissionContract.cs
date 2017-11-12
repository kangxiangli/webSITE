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
    /// <summary>
    /// 业务层接口
    /// </summary>
    public interface IProductCommissionContract : IDependency
    {
        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<ProductCommission> ProductCommissions { get; }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        OperationResult Recovery(params int[] ids);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult Insert(params ProductCommissionDto[] dto);

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        ProductCommissionDto Edit(int Id);

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult Update(params ProductCommissionDto[] dto);

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        ProductCommission View(int Id);
    }
}
