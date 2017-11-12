using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberBehaviorContract : IBaseContract<MemberBehavior>
    {
        /// <summary>
        /// 添加会员浏览商品记录
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="ProductId"></param>
        /// <returns></returns>
        OperationResult AddBehaviorRecord(int MemberId, string BigProNum);
        /// <summary>
        /// 根据浏览记录获取相关推荐
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        OperationResult RelatedRecommend(int MemberId, int StoreId, int Count = 10);
        /// <summary>
        /// 获取我的浏览记录
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        OperationResult GetBehaviourRecord(int MemberId, int PageIndex = 1, int PageSize = 10);

    }
}
