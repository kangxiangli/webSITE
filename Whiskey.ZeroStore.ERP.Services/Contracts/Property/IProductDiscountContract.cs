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
    /// <summary>
    /// 业务层接口
    /// </summary>
    public interface IProductDiscountContract : IDependency
    {
                  
        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<ProductDiscount> ProductDiscounts{get;}

        List<ProductDiscountDto> ProductDiscountDtos(Expression<Func<ProductDiscount, bool>> exp);
            
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult Insert(params ProductDiscountDto[] dto);
        /// <summary>
        /// 逻辑移除
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        OperationResult Remove(params int[] ids);
        /// <summary>
        /// 物理删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);
        
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        OperationResult Update(params ProductDiscountDto[] dto);

        /// <summary>
        /// 获取编辑对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        ProductDiscountDto Edit(int Id);

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        ProductDiscount View(int Id);
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        OperationResult Recovery(params int[] ids);


        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
    }
}
