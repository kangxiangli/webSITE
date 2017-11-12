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
    public interface ICartContract : IDependency
    {
        #region ICartContract

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<Cart> Carts { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Insert(params CartDto[] dtos);

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Update(params CartDto[] dtos);





        #endregion
    }
}
