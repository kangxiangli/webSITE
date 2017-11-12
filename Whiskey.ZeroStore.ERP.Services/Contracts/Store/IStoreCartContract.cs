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
    public interface IStoreCartContract : IDependency
    {
        #region StoreCart

        StoreCart View(int Id);

        StoreCartDto Edit(int Id);

        IQueryable<StoreCart> StoreCarts { get; }

        OperationResult Insert(params StoreCartDto[] dtos);

        OperationResult Update(params StoreCartDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);
        
        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult AddCart(int? StoreCartId, params StoreCartItemDto[] infos);
        OperationResult AddCart(int AdminId, int? StoreCartId, params StoreCartItemDto[] infos);
        OperationResult AddCartAuto(int AdminId, StoreCartDto dtoCart, params StoreCartItemDto[] infos);

        OperationResult AddPurchase(int ReceiptStorageId, int ReceiptStoreId, bool WithoutMoney = false);

        OperationResult AddPurchase(int AdminId, int ReceiverStorageId, int ReceiverStoreId, bool WithoutMoney = false);
        /// <summary>
        /// 直接下单
        /// </summary>
        /// <param name="CartId">购物车Id</param>
        /// <returns></returns>
        OperationResult AddPurchaseDirect(int? AdminId, int? CartId);
        #endregion

        OperationResult RemoveAll();

        OperationResult RemoveAll(int AdminId);

        float GetStoreDepositDiscount(int storeId);
    }
}
