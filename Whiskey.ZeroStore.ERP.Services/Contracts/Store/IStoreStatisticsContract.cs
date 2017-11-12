using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IStoreStatisticsContract : IDependency
    {
        IQueryable<StoreStatistics> StoreStatistics { get; }

        OperationResult Insert(params StoreStatistics[] ents);

        /// <summary>
        /// 按日期来删除统计数据
        /// 日期格式:20160816
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        OperationResult Delete(params int[] ids);

        StoreStatistics StatData(int storeId, string dateInt);

        DbContextTransaction GetTransaction();

        /// <summary>
        /// 签退时统计当日店铺各项数据
        /// </summary>
        OperationResult StatStoreWhenSignOut(int adminId);

        /// <summary>
        /// 员工首次签到,更新店铺close状态
        /// </summary>
        OperationResult SetStoreOpenWhenFirstSignIn(int adminId);


        /// <summary>
        /// 统计某一天的零售单于退货单数量
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="dateInt">日期</param>
        OperationResult StatOrderCount();

    }
}