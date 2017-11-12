
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IGameContract : IBaseContract<Game, GameDto>
    {
        /// <summary>
        /// 通过游戏获得积分
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="GameId"></param>
        /// <param name="Score"></param>
        /// <returns></returns>
        OperationResult AddScore(int MemberId, string Tag, decimal Score);
        /// <summary>
        /// 游戏分享
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        OperationResult Share(int MemberId, string Tag, ShareFlag flag = ShareFlag.游戏);
        /// <summary>
        /// 玩游戏使用概率,获得积分后直接充值
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        OperationResult PlayRandom(int MemberId, string Tag);

        /// <summary>
        /// 随机获取指定个数获奖信息【含有虚假数据】
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        OperationResult GetRandomAward(string Tag, int Count = 3);

    }
}

