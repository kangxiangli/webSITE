using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IPartnerManageContract : IBaseContract<PartnerManage, PartnerManageDto>
    {
        /// <summary>
        /// 申请加盟商,碟探使用，非会员主动申请
        /// </summary>
        /// <returns></returns>
        OperationResult JoinUs(PartnerManageDto dto);
    }
}
