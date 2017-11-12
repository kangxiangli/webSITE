using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IOrderblankContract : IDependency
    {
        IQueryable<Orderblank> Orderblanks { get; }
        OperationResult Insert(Orderblank[] ordeblankArr);
        OperationResult Delete(int orderblankArr);
        OperationResult Update(params Orderblank[] orderblankArr);

        OperationResult UpdateCheckstat(string Number);



        OperationResult Insert(bool checkUnFinished, params OrderblankDto[] dtos);

        OperationResult Refuse(string num, string msg, Action<OrderblankNoticeModel> sendNotice);

        /// <summary>
        /// 获取指定仓库下未完成配货单的数量
        /// </summary>
        /// <param name="storageIds"></param>
        /// <returns></returns>
        OperationResult<int> GetInCompleteOrderblankCount(int[] storageIds);

        DbContextTransaction GetTransaction();

        OperationResult LogWhenOrderblankRemove(params Inventory[] inventoryList);

        OperationResult LogWhenOrderblankAdd(Orderblank orderblankEntity, params Inventory[] inventoryList);

        OperationResult LogWhenOrderblankDelivery(Orderblank orderblankEntity, params Inventory[] inventoryList);

        OperationResult LogWhenOrderblankAccept(Orderblank orderblankEntity, params Inventory[] inventoryList);

        OperationResult LogWhenOrderblankDrop(params Inventory[] inventoryList);

        /// <summary>
        /// 采购单生成配货单
        /// </summary>
        /// <param name="purchaseId"></param>
        /// <returns></returns>
        OperationResult SaveOrderblankFromPurchaseOrder(int? purchaseId);

        /// <summary>
        /// 预约采购单生成配货单
        /// </summary>
        /// <param name="packingId"></param>
        /// <returns></returns>
        OperationResult SaveOrderblankFromAppointmentPacking(int packingId);

        /// <summary>
        /// 根据配货单关联的发货,收货店铺状态,判断是否可进行配货单的各种操作
        /// </summary>
        OperationResult CheckOrderbalnkAction(Orderblank orderblankEntity, OrderblankAction action);

        void TimeoutProcess(PunishTypeEnum punishType, string number, int deductScore);

        /// <summary>
        /// 配货单确认收货
        /// </summary>
        /// <param name="orderblankNumber">配货单</param>
        /// <returns></returns>
        OperationResult ReceptProduct(string orderblankNumber);

    }
}
