using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IStatisticsContract : IDependency
    {
        OperationResult GetSomeCount(int? FrontDay, int TopCount = 6);
        OperationResult GetManagedStores(int AdminId, string HitName = "0-FASHION");
        OperationResult GetSaleInfo(int AdminId, int StoreId, int? CategoryId, int FrontDay = 7);
        OperationResult GetCategoryInfo(int AdminId, int StoreId, int? CategoryId);
        OperationResult GetStorageInfo(int AdminId, int StoreId, int FrontDay = 7);
        /// <summary>
        /// 获取店铺位置开闭店及开店排行榜相关信息
        /// </summary>
        /// <param name="_storeContract"></param>
        /// <param name="StoreId"></param>
        /// <param name="TopCount"></param>
        /// <returns></returns>
        OperationResult GetStoreLocationInfo(int? StoreId, int TopCount = 3);

        OperationResult GetCategorySaleStatInfo(int adminId, int storeId);

        /// <summary>
        /// 获取统计options
        /// </summary>
        OperationResult QueryOptions();

        /// <summary>
        /// 获取指定店铺指定款号下的所有颜色和尺码
        /// </summary>
        OperationResult QueryOptions(int? storeId, string bigProductNum);


        OperationResult SaleStat(SaleStatReq req);

        /// <summary>
        /// 品牌统计
        /// </summary>
        OperationResult BrandStat(BrandStatReq req);

        /// <summary>
        /// 品类统计
        /// </summary>
        OperationResult CategoryStat(SaleStatReq req);



        /// <summary>
        /// 款号统计
        /// </summary>
        OperationResult BigProductNumStat(BigProductNumStatReq req);



        /// <summary>
        /// 会员统计
        /// </summary>
        OperationResult MemberStat(MemberStatReq req);

    }
}
