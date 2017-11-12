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
    public interface IStoreRecommendContract : IDependency
    {
        IQueryable<StoreRecommend> StoreRecommends { get; }
        OperationResult Insert(params StoreRecommend[] entities);
        OperationResult Insert(params StoreRecommendDto[] dtos);
        OperationResult Disable(int[] ids);
        OperationResult Enable(int[] ids);
        OperationResult Delete(int[] ids);
        OperationResult Recovery(int[] ids);
        OperationResult Update(params StoreRecommend[] entities);
        OperationResult SaveRecommend(string numbers);
        OperationResult DeleteRecommend(string number);
        OperationResult UpdateRecommendStoreId(string number, string storeIds);


        /// <summary>
        /// 获取时间配置
        /// </summary>
        /// <returns></returns>
        BigProdNumStateConfigEntry GetConfig();
       



        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="entry">配置对象</param>
        /// <returns></returns>
        OperationResult UpdateConfig(BigProdNumStateConfigEntry entry);
        





        /// <summary>
        /// 批量根据商城款号查询新品状态
        /// </summary>
        /// <param name="bigProdNums">款号</param>
        /// <returns>新品状态</returns>
        Dictionary<string, BigProdNumStateEnum> GetOnlineStoreBigProdNumState(BigProdNumStateConfigEntry config, params string[] bigProdNums);



        /// <summary>
        /// 获取所有商城款号新品状态
        /// </summary>
        /// <returns>新品状态</returns>
        Dictionary<string, BigProdNumStateEnum> GetOnlineStoreBigProdNumState();


    }
}
