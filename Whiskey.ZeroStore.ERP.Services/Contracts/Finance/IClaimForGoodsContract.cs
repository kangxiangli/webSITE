
using System;
using System.Collections.Generic;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IClaimForGoodsContract : IBaseContract<ClaimForGoods, ClaimForGoodsDto>
    {
        /// <summary>
        /// 申领物品
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        OperationResult Apply(ClaimForGoodsDto dto);

        /// <summary>
        /// 归还物品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        OperationResult ReturnGoods(int[] Ids);

        /// <summary>
        /// 归还提醒
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        OperationResult ReturnReminder(int[] Ids, Action<List<int>> sendNotificationAction);
    }
}

