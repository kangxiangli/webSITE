
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IGameContract : IBaseContract<Game, GameDto>
    {
        /// <summary>
        /// ͨ����Ϸ��û���
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="GameId"></param>
        /// <param name="Score"></param>
        /// <returns></returns>
        OperationResult AddScore(int MemberId, string Tag, decimal Score);
        /// <summary>
        /// ��Ϸ����
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        OperationResult Share(int MemberId, string Tag, ShareFlag flag = ShareFlag.��Ϸ);
        /// <summary>
        /// ����Ϸʹ�ø���,��û��ֺ�ֱ�ӳ�ֵ
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        OperationResult PlayRandom(int MemberId, string Tag);

        /// <summary>
        /// �����ȡָ����������Ϣ������������ݡ�
        /// </summary>
        /// <param name="Tag"></param>
        /// <param name="Count"></param>
        /// <returns></returns>
        OperationResult GetRandomAward(string Tag, int Count = 3);

    }
}

