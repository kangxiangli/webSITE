using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IShoppingCartItemContract:IDependency
    {

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<ShoppingCartItem> Entities { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(params ShoppingCartItem[] entities);



        OperationResult Replace(int memberId, ShoppingCartUpdateDto[] dtos);

        


        /// <summary>
        /// 获取购物车
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        List<ShoppingCartEntry> GetItems(int memberId);



        /// <summary>
        /// 数量修改
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="productNumber"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        OperationResult UpdateQuantity(int memberId, string productNumber, int quantity);

        OperationResult AddItem(int memberId,params ShoppingCartUpdateDto[] dtos);

        OperationResult DelItem(int memberId, params string[] productNumber);

        OperationResult ClearItem(int memberId);
    }
}
