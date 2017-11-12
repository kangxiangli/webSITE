
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberShareContract : IBaseContract<MemberShare, MemberShareDto>
    {
        /// <summary>
        /// 查看今日分享次数
        /// </summary>
        /// <returns></returns>
        int ShareCountToday(int MemberId, ShareFlag Flag);
    }
}

