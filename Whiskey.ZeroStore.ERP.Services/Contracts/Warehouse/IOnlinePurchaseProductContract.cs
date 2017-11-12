using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IOnlinePurchaseProductContract : IDependency
    {
        #region OnlinePurchaseProduct

        OnlinePurchaseProduct View(int Id);

        OnlinePurchaseProductDto Edit(int Id);

        IQueryable<OnlinePurchaseProduct> OnlinePurchaseProducts { get; }

        IQueryable<OnlinePurchaseProductItem> OnlinePurchaseProductItems { get; }

        OperationResult Insert(params OnlinePurchaseProductDto[] dtos);

        OperationResult Update(params OnlinePurchaseProductDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        #endregion

        /// <summary>
        /// 添加可采购商品详情
        /// </summary>
        /// <param name="UniqueCode">唯一标识</param>
        /// <param name="list"></param>
        /// <returns></returns>
        OperationResult Insert(string UniqueCode, CheckResultEdo edo);


        OperationResult Remove(string UniqueCode, string[] BigProdNums);

        BigProdNumStateConfigEntry GetConfig();


        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="entry">配置对象</param>
        /// <returns></returns>
        OperationResult UpdateConfig(BigProdNumStateConfigEntry entry);


        /// <summary>
        /// 根据选货单创建时间,获取选货单的新品状态
        /// </summary>
        /// <param name="createdTime">选货单创建时间</param>
        /// <param name="config">配置对象,由外部传入,批量操作时,注意避免在循环中反复从缓存中读取此配置</param>
        /// <returns>新品状态</returns>
        BigProdNumStateEnum GetOnlinePurchaseState(DateTime createdTime, BigProdNumStateConfigEntry config);



        /// <summary>
        /// 批量根据款号查询新品状态
        /// </summary>
        /// <param name="bigProdNums">款号</param>
        /// <returns>新品状态</returns>
        Dictionary<string, BigProdNumStateEnum> GetOnlinePurchaseBigProdNumState(BigProdNumStateConfigEntry config, params string[] bigProdNums);



        /// <summary>
        /// 获取所有选货单内款号新品状态
        /// </summary>
        /// <returns>新品状态</returns>
        Dictionary<string, BigProdNumStateEnum> GetOnlinePurchaseBigProdNumState();
    }
}
